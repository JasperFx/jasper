using System;
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

        Task<ErrorReport> LoadDeadLetterEnvelope(Guid id);
        Task<Envelope[]> AllIncomingEnvelopes();
        Task<Envelope[]> AllOutgoingEnvelopes();


        Task ReleaseAllOwnership();
    }
}
