using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;

namespace Jasper.RabbitMQ
{
    public class RabbitMqListenerConfiguration : ListenerConfiguration<RabbitMqListenerConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqListenerConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        public RabbitMqListenerConfiguration Protocol<T>() where T : IRabbitMqProtocol, new()
        {
            return Protocol(new T());
        }

        public RabbitMqListenerConfiguration Protocol(IRabbitMqProtocol protocol)
        {
            endpoint.Protocol = protocol;
            return this;
        }
    }
}