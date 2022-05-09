using System.Data.Common;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;

namespace Jasper.Persistence.Database;

public interface IDatabaseBackedEnvelopePersistence : IEnvelopePersistence
{
    public AdvancedSettings Settings { get; }

    public DatabaseSettings DatabaseSettings { get; }
    Task StoreIncoming(DbTransaction tx, Envelope[] envelopes);
    Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes);
}
