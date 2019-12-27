using System.Data;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurabilityAgentStorage : DurabilityAgentStorage
    {
        public SqlServerDurabilityAgentStorage(DatabaseSettings databaseSettings, AdvancedSettings settings) : base(databaseSettings, settings)
        {
        }

        protected override IDurableOutgoing buildDurableOutgoing(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return new SqlServerDurableOutgoing(durableStorageSession, databaseSettings, settings);
        }

        protected override IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return new SqlServerDurableIncoming(durableStorageSession, databaseSettings, settings);
        }
    }
}
