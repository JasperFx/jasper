using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;

namespace Jasper.RabbitMQ
{
    public class RabbitMqSubscriberConfiguration : SubscriberConfiguration<RabbitMqSubscriberConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqSubscriberConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        public RabbitMqSubscriberConfiguration Protocol<T>() where T : IRabbitMqProtocol, new()
        {
            return Protocol(new T());
        }

        public RabbitMqSubscriberConfiguration Protocol(IRabbitMqProtocol protocol)
        {
            _endpoint.Protocol = protocol;
            return this;
        }
    }
}