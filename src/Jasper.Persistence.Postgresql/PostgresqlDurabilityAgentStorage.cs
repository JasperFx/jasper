using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql.Util;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurabilityAgentStorage : DurabilityAgentStorage
    {
        public PostgresqlDurabilityAgentStorage(PostgresqlSettings settings, JasperOptions options) : base(settings, options)
        {

        }

        protected override IDurableOutgoing buildDurableOutgoing(DurableStorageSession durableStorageSession, DatabaseSettings settings,
            JasperOptions options)
        {
            return  new PostgresqlDurableOutgoing(durableStorageSession, settings, options);
        }

        protected override IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession, DatabaseSettings settings,
            JasperOptions options)
        {
            return new PostgresqlDurableIncoming(durableStorageSession, settings, options);
        }
    }
}
