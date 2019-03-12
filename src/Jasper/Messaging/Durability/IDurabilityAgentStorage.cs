using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
{
    /// <summary>
    /// Supports the IDurabilityAgent
    /// </summary>
    public interface IDurabilityAgentStorage : IDisposable
    {
        IDurableStorageSession Session { get; }
        IDurableNodes Nodes { get; }

        IDurableIncoming Incoming { get; }

        IDurableOutgoing Outgoing { get; }

        Task<Envelope[]> LoadScheduledToExecute(DateTimeOffset utcNow);
    }
}
