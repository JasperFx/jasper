using System;
using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
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
