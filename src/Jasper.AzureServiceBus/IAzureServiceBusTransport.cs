using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;

namespace Jasper.AzureServiceBus
{
    public interface IAzureServiceBusTransport
    {
        /// <summary>
        ///     Azure Service Bus connection string as read from configuration
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        ///     The Azure Service Bus RetryPolicy for this endpoint.
        /// </summary>
        RetryPolicy RetryPolicy { get; set; }

        /// <summary>
        ///     Default is Amqp
        /// </summary>
        TransportType TransportType { get; set; }

        /// <summary>
        ///     Set this for tokenized authentication
        /// </summary>
        ITokenProvider TokenProvider { get; set; }

        /// <summary>
        ///     Default is PeekLock
        /// </summary>
        ReceiveMode ReceiveMode { get; set; }
    }
}
