using Jasper.Messaging.Runtime;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus
{
    public interface IAzureServiceBusProtocol
    {
        Message WriteFromEnvelope(Envelope envelope);
        Envelope ReadEnvelope(Message message);
    }
}