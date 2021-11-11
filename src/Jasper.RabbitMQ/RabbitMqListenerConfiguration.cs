using System;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;

namespace Jasper.RabbitMQ
{
    public class RabbitMqListenerConfiguration : ListenerConfiguration<RabbitMqListenerConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqListenerConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        /// To optimize the message listener throughput,
        /// start up multiple listening endpoints. This is
        /// most necessary when using inline processing
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public RabbitMqListenerConfiguration ListenerCount(int count)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Must be greater than zero");

            endpoint.ListenerCount = count;
            return this;
        }

        /// <summary>
        /// Assume that any unidentified, incoming message types is the
        /// type "T". This is primarily for interoperability with non-Jasper
        /// applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RabbitMqListenerConfiguration DefaultIncomingMessage<T>()
        {
            return DefaultIncomingMessage(typeof(T));
        }

        /// <summary>
        /// Assume that any unidentified, incoming message types is the
        /// type "T". This is primarily for interoperability with non-Jasper
        /// applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RabbitMqListenerConfiguration DefaultIncomingMessage(Type messageType)
        {
            endpoint.ReceivesMessage(messageType);
            return this;
        }
    }
}
