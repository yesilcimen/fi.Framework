using fi.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace fi.RMQueueDLX
{
    public abstract class Service
    {
        protected readonly ILogger _logger;
        protected readonly IServiceScopeFactory _serviceScopeFactory;
        protected readonly string _connectionUrl;
        private readonly IDictionary<string, Subscriber> _subscribers;
        private readonly IModel _channel;

        public Service(string connectionUrl, ILogger<Service> logger, IServiceScopeFactory serviceScopeFactory)
        {
            if (connectionUrl.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(connectionUrl), "Null olamaz.");


            _logger = logger;
            _subscribers = new Dictionary<string, Subscriber>();
            _serviceScopeFactory = serviceScopeFactory;
            _connectionUrl = connectionUrl;
            _channel = CreateConnetion().CreateModel();
        }

        IConnection CreateConnetion()
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_connectionUrl),
                DispatchConsumersAsync = true,
            };

            return connectionFactory.CreateConnection();
        }

        public void Send<T>(string queueName, T data) where T : IQueueCommand
        {
            _channel.DeclareQueueWithDLX(queueName);

            var json = data.Serialize();
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: body);
        }

        public void Publish<T>(T data) where T : IQueueEvent
        {
            var json = data.Serialize();
            var body = Encoding.UTF8.GetBytes(json);

            var exchangeName = $"event.{data.GetType().Name}";

            _channel.ExchangeDeclareNoWait(exchangeName, ExchangeType.Fanout, durable: true, arguments: null);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);
        }


        public void PublishWithHeaders<T>(T data, Dictionary<string, object> headers, string exchangeName = null) where T : IQueueEvent
        {
            var json = data.Serialize();
            var body = Encoding.UTF8.GetBytes(json);

            exchangeName ??= $"event.{data.GetType().Name}";

            _channel.ExchangeDeclareNoWait(exchangeName, ExchangeType.Headers, durable: true, arguments: null);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Headers = headers;

            _channel.BasicPublish(exchange: exchangeName,
                                 routingKey: "",
                                 basicProperties: properties,
                                 body: body);
        }

        public Task SubscribeAsync<T>(string queueName, QueueConfigTemplate template, CancellationToken cancellationToken = default) where T : IConsumer
        {
            Subscribe(queueName, template, typeof(T), cancellationToken);

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
                    Subscribe(queues.Key, queue.ConfigTemplate, consumerMetadata, default, true, (uint)channel.ChannelNumber, channel);

                    _logger.LogInformation("Auto scaler created new instance. {Queue} {Consumer}", queues.Key, consumerMetadata.Name);
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
                            _logger.LogInformation("Auto scaler closed an instance. {Queue} {Consumer}", queues.Key, consumerMetadata.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Auto scaler could not close an instance. {Queue} {Consumer} ", queues.Key, consumerMetadata.Name);

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
                .SelectMany(sm => sm.Value.SubscribeInfos.Select(x => new { QueueName = sm.Key, Consumer = x.ConsumerMetadata.Name, x.Channel, x.Tag })).ToList();

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

        private void Subscribe(string queueName, QueueConfigTemplate template, Type type, CancellationToken cancellationToken = default, bool isScaleInstance = false, uint scaleNumber = 0, IModel channel = null)
        {
            _logger.LogInformation("{Queue} started consuming from {Consumer}. {@Template}", queueName, type.Name, template);

            IConnection conn = null;
            if (channel is null)
            {
                conn = CreateConnetion();
                channel = conn.CreateModel();
            }

            channel.DeclareQueueWithDLX(queueName);

            if (type.GetInterfaces().Any(x => x == typeof(IEventConsumer))) //IsEvent
            {
                var exchangeName = template.ExchangeName ?? $"event.{type.Name}";

                channel.ExchangeDeclareNoWait(exchangeName, template.ExchangeType, durable: true, arguments: null);

                channel.QueueBind(queueName, exchangeName, "", template.Headers);
            }

            var consumer = new AsyncEventingBasicConsumer(channel);

            channel.SetDelayedDeadLetterExchange(queueName);

            consumer.Received += async (model, ea) =>
            {
                //ToDo:@aeyesilcimen => aşağıdaki kod trygetvalue çalışmadığı için linq yazıldı. Tryget denemek için debug yapıp sonra kodu sabitleyin.29-11-2021
                if (!int.TryParse(
                    ea.BasicProperties.Headers?
                        .Where(w =>
                            w.Key.Equals("x-retry"))
                        .Select(s => s.Value.ToString())
                        .FirstOrDefault(), out int retryCount))
                    retryCount = -1;

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    using var scope = _serviceScopeFactory.CreateScope();
                    var instance = scope.ServiceProvider.GetRequiredService(type);
                    Console.WriteLine("Scope : {0}", stopwatch.Elapsed.TotalMilliseconds);

                    if (instance is not IConsumer consumer)
                    {
                        _logger.LogError("{Queue} {Consumer} not implemented IConsumer", queueName, type.Name);
                        await Task.Yield();
                        return;
                    }

                    await consumer.ConsumeAsync(message, scaleNumber, cancellationToken);

                    channel.BasicAck(ea.DeliveryTag, false);

                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Queue} {Consumer} Error", queueName, type.Name);

                    channel.ExecuteRetryPolicy(queueName, template, body, ea.DeliveryTag, retryCount, ex);
                    await Task.Yield();
                }

                await Task.Yield();
            };

            consumer.ConsumerCancelled += async (model, ea) =>
            {
                foreach (var item in _subscribers)
                    item.Value.SubscribeInfos.RemoveAll(x => x.Tag == ea.ConsumerTags[0]);

                await Task.Yield();
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
                ConsumerMetadata = type,
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
