using Jasper.Messaging.Runtime;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus
{
    // SAMPLE: IAzureServiceBusProtocol
    /// <summary>
    /// Used to "map" incoming Azure Service Bus Message objects to Jasper Envelopes. Can be implemented to
    /// connect Jasper to non-Jasper applications
    /// </summary>
    public interface IAzureServiceBusProtocol
    {
        /// <summary>
        /// Creates an Azure Service Bus Message object for a Jasper Envelope
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Message WriteFromEnvelope(Envelope envelope);

        /// <summary>
        /// Creates an Envelope for the incoming Azure Service Bus Message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Envelope ReadEnvelope(Message message);
    }
    // ENDSAMPLE
}
