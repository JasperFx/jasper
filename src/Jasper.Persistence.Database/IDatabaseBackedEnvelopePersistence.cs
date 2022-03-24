using System.Data.Common;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;

namespace Jasper.Persistence.Database
{
    public interface IDatabaseBackedEnvelopePersistence : IEnvelopePersistence
    {
        Task StoreIncoming(DbTransaction tx, Envelope[] envelopes);
        Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes);

        public AdvancedSettings Settings { get; }

        public DatabaseSettings DatabaseSettings { get; }
    }
}
