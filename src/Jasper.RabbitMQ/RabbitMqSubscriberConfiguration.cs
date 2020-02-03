using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;

namespace Jasper.RabbitMQ
{
    public class RabbitMqSubscriberConfiguration : SubscriberConfiguration<RabbitMqSubscriberConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqSubscriberConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RabbitMqSubscriberConfiguration Protocol<T>() where T : IRabbitMqProtocol, new()
        {
            return Protocol(new T());
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public RabbitMqSubscriberConfiguration Protocol(IRabbitMqProtocol protocol)
        {
            _endpoint.Protocol = protocol;
            return this;
        }


    }
}
