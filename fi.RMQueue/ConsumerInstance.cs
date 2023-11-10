﻿using System;
using System.Linq;

namespace fi.RMQueue
{
    public class ConsumerInstance
    {
        public static ConsumerMetadata GetConsumerMetadata<T>()
        {
            var type = typeof(T);

            return GetConsumerMetadata(type);
        }

        public static ConsumerMetadata GetConsumerMetadata(Type type)
        {
            try
            {
                var interfaceType = type.GetInterfaces().Where(x => x.GetGenericTypeDefinition() == typeof(IConsumer<>)).First();
                var messageType = interfaceType.GenericTypeArguments.First();
                var method = type.GetMethod("Consume");

                var isEvent = messageType.GetInterfaces().Any(x => x == typeof(IQueueEvent));

                return new ConsumerMetadata
                {
                    IsEvent = isEvent,
                    MessageType = messageType,
                    Method = method,
                    ConsumerType = type
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Geçersiz consumer. Queue modelini kontrol edin veya consumerı inject edin", ex);
            }
        }
    }
}
