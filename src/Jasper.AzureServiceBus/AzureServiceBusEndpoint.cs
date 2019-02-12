using Jasper.AzureServiceBus.Internal;
using Jasper.Messaging.Transports;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;

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

        public RetryPolicy RetryPolicy { get; set; } = Microsoft.Azure.ServiceBus.RetryPolicy.Default;

        public TransportType TransportType { get; set; } = TransportType.Amqp;

        public ITokenProvider TokenProvider { get; set; }

        public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
    }
}
