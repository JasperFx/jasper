using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql.Util;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurabilityAgentStorage : DurabilityAgentStorage
    {
        public PostgresqlDurabilityAgentStorage(PostgresqlSettings databaseSettings, AdvancedSettings settings) : base(databaseSettings, settings)
        {

        }

        protected override IDurableOutgoing buildDurableOutgoing(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return  new PostgresqlDurableOutgoing(durableStorageSession, databaseSettings, settings);
        }

        protected override IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return new PostgresqlDurableIncoming(durableStorageSession, databaseSettings, settings);
        }
    }
}
