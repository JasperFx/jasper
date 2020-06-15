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
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RabbitMqListenerConfiguration Protocol<T>() where T : IRabbitMqProtocol, new()
        {
            return Protocol(new T());
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public RabbitMqListenerConfiguration Protocol(IRabbitMqProtocol protocol)
        {
            endpoint.Protocol = protocol;
            return this;
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
    }
}
