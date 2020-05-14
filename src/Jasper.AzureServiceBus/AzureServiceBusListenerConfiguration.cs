using Jasper.Configuration;
using Jasper.Transports;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusListenerConfiguration : ListenerConfiguration<AzureServiceBusListenerConfiguration, AzureServiceBusEndpoint>
    {
        public AzureServiceBusListenerConfiguration(AzureServiceBusEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AzureServiceBusListenerConfiguration Protocol<T>() where T : ITransportProtocol<Message>, new()
        {
            return Protocol(new T());
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public AzureServiceBusListenerConfiguration Protocol(ITransportProtocol<Message> protocol)
        {
            endpoint.Protocol = protocol;
            return this;
        }
    }
}
