using System;
using System.Collections.Generic;
using System.Net.Http;

namespace fi.RMQueue
{
    public class QueueConfigTemplate
    {

        public ushort PrefetchCount { get; set; }
        public ushort RetryCount { get; set; }
        public int RetryIntervalSeconds { get; set; }
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Fanout;
        public string ExchangeName { get; set; }
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
        public ICollection<Type> ExcludeExceptions { get; }
        /// <summary>
        /// Eğer true setlerseniz Autoscaler abstract klasından, scaler worker ınızı oluşturmanız gerekmektedir.
        /// </summary>
        public bool AutoScale { get; set; }
        public ushort ScaleUpTo { get; set; }

        public QueueConfigTemplate()
        {
            this.ExcludeExceptions = new HashSet<Type> { typeof(Exception), typeof(HttpRequestException), typeof(ArgumentNullException) };
        }
        public static QueueConfigTemplate Default()
        {
            return new QueueConfigTemplate
            {
                PrefetchCount = 1,
                RetryCount = 3,
                RetryIntervalSeconds = 10,
                AutoScale = false,
                ExchangeType = RabbitMQ.Client.ExchangeType.Fanout,
                ScaleUpTo = 0 // add new Scale
            };
        }
    }

}
