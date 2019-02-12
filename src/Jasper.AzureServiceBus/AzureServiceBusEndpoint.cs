using Jasper.AzureServiceBus.Internal;
using Jasper.Messaging.Transports;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusEndpoint : Endpoint<IAzureServiceBusProtocol>
    {
        public AzureServiceBusEndpoint(TransportUri uri, string connectionString) : base(uri, new DefaultAzureServiceBusProtocol())
        {
            ConnectionString = connectionString;
        }

        public override void Dispose()
        {
            // Nothing going on here
        }

        public string ConnectionString { get; }
    }
}