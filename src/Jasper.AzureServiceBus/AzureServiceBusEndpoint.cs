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

        /// <summary>
        /// Azure Service Bus connection string as read from configuration
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// The Azure Service Bus RetryPolicy for this endpoint.
        /// </summary>
        public RetryPolicy RetryPolicy { get; set; } = Microsoft.Azure.ServiceBus.RetryPolicy.Default;

        /// <summary>
        /// Default is Amqp
        /// </summary>
        public TransportType TransportType { get; set; } = TransportType.Amqp;

        /// <summary>
        /// Set this for tokenized authentication
        /// </summary>
        public ITokenProvider TokenProvider { get; set; }

        /// <summary>
        /// Default is PeekLock
        /// </summary>
        public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
    }
}
