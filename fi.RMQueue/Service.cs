using fi.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fi.RMQueue
{
    public abstract class Service
    {
        protected readonly ILogger _logger;
        protected readonly IConnection _connection;
        protected readonly IServiceScopeFactory _serviceScopeFactory;
        protected readonly string _connectionUrl;
        private readonly IDictionary<string, Subscriber> _subscribers;

        public Service(string connectionUrl, ILogger<Service> logger, IServiceScopeFactory serviceScopeFactory)
        {
            if (connectionUrl.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(connectionUrl), "Null olamaz.");


            _logger = logger;
            _subscribers = new Dictionary<string, Subscriber>();
            _serviceScopeFactory = serviceScopeFactory;
            _connectionUrl = connectionUrl;
            _connection = CreateConnetion();
        }

        IConnection CreateConnetion()
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_connectionUrl)
            };

            return connectionFactory.CreateConnection();
        }

        public void Send<T>(string queueName, T data) where T : IQueueCommand
        {
            using var channel = _connection.CreateModel();
            channel.DeclareQueueWithDLX(queueName);

            var json = data.Serialize();
            var body = Encoding.UTF8.GetBytes(json);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: body);
        }

        public void Publish<T>(T data) where T : IQueueEvent
        {
            using var channel = _connection.CreateModel();
            var json = data.Serialize();
            var body = Encoding.UTF8.GetBytes(json);

            var exchangeName = $"event.{data.GetType().Name}";

            channel.ExchangeDeclareNoWait(exchangeName, ExchangeType.Fanout, durable: true, arguments: null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);
        }


        public void PublishWithHeaders<T>(T data, Dictionary<string, object> headers, string exchangeName = null) where T : IQueueEvent
        {
            using var channel = _connection.CreateModel();
            var json = data.Serialize();
            var body = Encoding.UTF8.GetBytes(json);

            exchangeName ??= $"event.{data.GetType().Name}";

            channel.ExchangeDeclareNoWait(exchangeName, ExchangeType.Headers, durable: true, arguments: null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Headers = headers;

            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);
        }

        public Task Subscribe<T>(string queueName, QueueConfigTemplate template) where T : IConsumer
        {
            var consumerMetadata = ConsumerInstance.GetConsumerMetadata<T>();

            Subscribe(queueName, template, consumerMetadata);

            return Task.CompletedTask;
        }

        public Task Subscribe(Type type, string queueName, QueueConfigTemplate template)
        {
            var consumerMetadata = ConsumerInstance.GetConsumerMetadata(type);

            Subscribe(queueName, template, consumerMetadata);

            return Task.CompletedTask;
        }

        public void AutoScale()
        {

            foreach (var queues in _subscribers)
            {
                var queue = queues.Value;

                var channel = queue.Connection.CreateModel();

                var count = channel.MessageCount(queues.Key);

                var averageConsumptionPerConsumer = 20;

                var idealConsumerCount = Math.Ceiling((decimal)count / averageConsumptionPerConsumer);

                var consumerMetadata = queue.SubscribeInfos.First().ConsumerMetadata;

                if (queue.ConfigTemplate.AutoScale && idealConsumerCount > channel.ChannelNumber && channel.ChannelNumber <= queue.ConfigTemplate.ScaleUpTo)
                {
                    Subscribe(queues.Key, queue.ConfigTemplate, consumerMetadata, true, (uint)channel.ChannelNumber, channel);

                    _logger.LogInformation("Auto scaler created new instance. {Queue} {Consumer}", queues.Key, consumerMetadata.ConsumerType.Name);
                }

                else if (idealConsumerCount < channel.ChannelNumber && queue.SubscribeInfos.Where(x => x.IsScaleInstance).Any())
                {
                    channel.Dispose();
                    var cancellingConsumer = queue.SubscribeInfos.Where(x => x.IsScaleInstance).FirstOrDefault();

                    try
                    {
                        cancellingConsumer.Channel.BasicCancel(cancellingConsumer.Tag);
                        Thread.Sleep(1000);
                        if (!cancellingConsumer.Channel.IsClosed)
                        {
                            cancellingConsumer.Channel.Close();
                            _logger.LogInformation("Auto scaler closed an instance. {Queue} {Consumer}", queues.Key, consumerMetadata.ConsumerType.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Auto scaler could not close an instance. {Queue} {Consumer} ", queues.Key, consumerMetadata.ConsumerType.Name);

                        queue.SubscribeInfos.RemoveAll(x => x.Tag == queue.SubscribeInfos.First().Tag);
                    }
                }
                else
                {
                    channel.Dispose();
                }
            }
        }

        public void StopConsumers()
        {
            var activeConsumers = _subscribers
                .SelectMany(sm => sm.Value.SubscribeInfos.Select(x => new { QueueName = sm.Key, Consumer = x.ConsumerMetadata.ConsumerType.Name, x.Channel, x.Tag })).ToList();

            foreach (var item in activeConsumers)
            {
                try
                {
                    item.Channel.BasicCancel(item.Tag);

                    _logger.LogInformation("Auto scaler closed an instance. {Queue} {Consumer}", item.QueueName, item.Consumer);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Auto scaler could not close an instance. {Queue} {Consumer} ", item.QueueName, item.Consumer);

                    throw;
                }
            }

            _subscribers.Clear();
        }

        private void Subscribe(string queueName, QueueConfigTemplate template, ConsumerMetadata consumerMetadata, bool isScaleInstance = false, uint scaleNumber = 0, IModel channel = null)
        {
            _logger.LogInformation("{Queue} started consuming from {Consumer}. {@Template}", queueName, consumerMetadata.ConsumerType.Name, template);

            IConnection conn = null;
            if (channel is null)
            {
                conn = CreateConnetion();
                channel = conn.CreateModel();
            }

            channel.DeclareQueueWithDLX(queueName);

            if (consumerMetadata.IsEvent)
            {
                var exchangeName = template.ExchangeName ?? $"event.{consumerMetadata.MessageType.Name}";

                channel.ExchangeDeclareNoWait(exchangeName, template.ExchangeType, durable: true, arguments: null);

                channel.QueueBind(queueName, exchangeName, "", template.Headers);
            }

            var consumer = new EventingBasicConsumer(channel);

            channel.SetDelayedDeadLetterExchange(queueName);

            consumer.Received += (model, ea) =>
            {
                object queueModel = null;

                //ToDo:@aeyesilcimen => aşağıdaki kod trygetvalue çalışmadığı için linq yazıldı. Tryget denemek için debug yapıp sonra kodu sabitleyin.29-11-2021
                if (!int.TryParse(ea.BasicProperties.Headers?.Where(w => w.Key.Equals("x-retry")).Select(s => s.Value.ToString()).FirstOrDefault(), out int retryCount))
                    retryCount = -1;

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    queueModel = message.Deserialize(consumerMetadata.MessageType);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "{Queue} Json convert error. Message moved to fault queue", queueName);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.Headers = new Dictionary<string, object>(template.Headers)
                    {
                        { "Error", exception.Message }
                    };

                    channel.MoveToFaultQueue(queueName, ea.DeliveryTag, body, properties);

                    return;
                }

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var instance = scope.ServiceProvider.GetRequiredService(consumerMetadata.ConsumerType);
                    consumerMetadata.Method.Invoke(instance, new[] { queueModel, scaleNumber });

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Queue} {Consumer} Error", queueName, consumerMetadata.ConsumerType.Name);

                    channel.ExecuteRetryPolicy(queueName, template, body, ea.DeliveryTag, retryCount, ex);
                }
            };

            consumer.ConsumerCancelled += (model, ea) =>
            {
                foreach (var item in _subscribers)
                    item.Value.SubscribeInfos.RemoveAll(x => x.Tag == ea.ConsumerTags[0]);
            };

            channel.ModelShutdown += (model, ea) =>
            {
                _logger.LogWarning("{Queue} Rabbitmq model shutdown. {Reason}", queueName, ea.ReplyText);
            };

            channel.CallbackException += (model, ea) =>
            {
                _logger.LogError(ea.Exception, "{Queue} Rabbitmq callback exception occured.", queueName);
            };

            channel.BasicQos(0, template.PrefetchCount, false);

            var tag = channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            SubscribeInfo subscriberInfo = new()
            {
                Channel = channel,
                ConsumerMetadata = consumerMetadata,
                IsScaleInstance = isScaleInstance,
                Tag = tag
            };


            if (_subscribers.TryGetValue(queueName, out Subscriber subscriber))
            {
                subscriber.SubscribeInfos.Add(subscriberInfo);
            }
            else
            {
                _subscribers.Add(queueName, new Subscriber
                {
                    Connection = conn,
                    ConfigTemplate = template,
                    SubscribeInfos = new List<SubscribeInfo> { subscriberInfo }
                });
            }
        }
    }
}
