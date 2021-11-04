using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Logging;

namespace Jasper.Persistence.Durability
{
    public interface IEnvelopeStorageAdmin
    {
        Task ClearAllPersistedEnvelopes();
        Task RebuildSchemaObjects();
        string CreateSql();
        Task<PersistedCounts> GetPersistedCounts();

        Task<IReadOnlyList<Envelope>> AllIncomingEnvelopes();
        Task<IReadOnlyList<Envelope>> AllOutgoingEnvelopes();


        Task ReleaseAllOwnership();
    }
}
