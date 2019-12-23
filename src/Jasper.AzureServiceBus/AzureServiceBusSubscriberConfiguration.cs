using Jasper.Configuration;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusSubscriberConfiguration : SubscriberConfiguration<AzureServiceBusSubscriberConfiguration, AzureServiceBusEndpoint>
    {
        public AzureServiceBusSubscriberConfiguration(AzureServiceBusEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AzureServiceBusSubscriberConfiguration Protocol<T>() where T : IAzureServiceBusProtocol, new()
        {
            return Protocol(new T());
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public AzureServiceBusSubscriberConfiguration Protocol(IAzureServiceBusProtocol protocol)
        {
            _endpoint.Protocol = protocol;
            return this;
        }
    }
}
