using System.Data;
using Jasper.Messaging.Durability;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurabilityAgentStorage : DurabilityAgentStorage
    {
        public SqlServerDurabilityAgentStorage(DatabaseSettings settings, JasperOptions options) : base(settings, options)
        {
        }

        protected override IDurableOutgoing buildDurableOutgoing(DurableStorageSession durableStorageSession, DatabaseSettings settings,
            JasperOptions options)
        {
            return new SqlServerDurableOutgoing(durableStorageSession, settings, options);
        }

        protected override IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession, DatabaseSettings settings,
            JasperOptions options)
        {
            return new SqlServerDurableIncoming(durableStorageSession, settings, options);
        }
    }
}
