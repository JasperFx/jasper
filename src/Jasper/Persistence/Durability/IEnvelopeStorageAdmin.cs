using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Logging;

namespace Jasper.Persistence.Durability
{
    public interface IEnvelopeStorageAdmin
    {
        [Obsolete("remove in favor of Oakton resource model")]
        Task ClearAllPersistedEnvelopes();

        [Obsolete("remove in favor of Oakton resource model")]
        Task RebuildStorageAsync();
        string ToDatabaseScript();
        Task<PersistedCounts> GetPersistedCounts();

        Task<IReadOnlyList<Envelope>> AllIncomingEnvelopes();
        Task<IReadOnlyList<Envelope>> AllOutgoingEnvelopes();


        Task ReleaseAllOwnership();
    }
}
