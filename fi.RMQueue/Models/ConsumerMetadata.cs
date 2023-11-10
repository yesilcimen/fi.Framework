using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace fi.RMQueue
{
    public class ConsumerMetadata
    {
        public Type ConsumerType { get; set; }
        public MethodInfo Method { get; set; }
        public Type MessageType { get; set; }
        public bool IsEvent { get; set; }
    }
    public interface IConsumer { }
    public interface IQueueCommand { }
    public interface IQueueEvent { }
    public interface IConsumer<T> : IConsumer where T : class
    {
        void Consume(T message, uint scaleNumber);
    }
    public class Subscriber
    {
        //public string QueueName { get; set; }
        public IConnection Connection { get; set; }
        public QueueConfigTemplate ConfigTemplate { get; set; }
        public List<SubscribeInfo> SubscribeInfos { get; set; }
    }

    public class SubscribeInfo
    {
        public bool IsScaleInstance { get; set; }
        public IModel Channel { get; set; }
        public string Tag { get; set; }
        public ConsumerMetadata ConsumerMetadata { get; set; }
    }
}