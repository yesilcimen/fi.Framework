using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace fi.RMQueueDLX
{
    public class ConsumerMetadata
    {
        public Type ConsumerType { get; set; }
        public MethodInfo Method { get; set; }
        public Type MessageType { get; set; }
        public bool IsEvent { get; set; }
    }
    public interface IConsumer { 
        Task ConsumeAsync(string textJson, uint scaleNumber, CancellationToken cancellationToken = default);
    }
    public interface IEventConsumer : IConsumer { }
    public interface IQueueCommand { }
    public interface IQueueEvent { }
    public class Subscriber
    {
        public IConnection Connection { get; set; }
        public QueueConfigTemplate ConfigTemplate { get; set; }
        public List<SubscribeInfo> SubscribeInfos { get; set; }
    }

    public class SubscribeInfo
    {
        public bool IsScaleInstance { get; set; }
        public IModel Channel { get; set; }
        public string Tag { get; set; }
        public Type ConsumerMetadata { get; set; }
    }
}