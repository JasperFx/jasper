using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;

namespace Jasper.Runtime
{
    public interface IExecutionContext : IAcknowledgementSender
    {
        Task SendAllQueuedOutgoingMessages();
        IEnvelopePersistence Persistence { get; }
        IMessageLogger Logger { get; }

        IMessagingRoot Root { get; }

        Envelope Envelope { get; }
    }
}
