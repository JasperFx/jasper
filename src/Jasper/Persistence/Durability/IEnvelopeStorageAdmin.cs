using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Logging;

namespace Jasper.Persistence.Durability
{
    public interface IEnvelopeStorageAdmin
    {
        Task ClearAllPersistedEnvelopes();

        Task RebuildStorageAsync();
        Task<PersistedCounts> GetPersistedCounts();

        Task<IReadOnlyList<Envelope>> AllIncomingEnvelopes();
        Task<IReadOnlyList<Envelope>> AllOutgoingEnvelopes();


        Task ReleaseAllOwnershipAsync();

        public Task CheckAsync(CancellationToken token);
    }
}
