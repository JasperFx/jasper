using System;
using IntegrationTests;
using Jasper.Persistence.Postgresql;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Persistence.Testing.Postgresql
{

    public class PostgresBackedLocal : JasperOptions
    {
        public PostgresBackedLocal()
        {
            Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString);


        }
    }

    public class LocalPostgresqlBackedFixture : SendingComplianceFixture
    {
        public LocalPostgresqlBackedFixture() : base("local://one/durable".ToUri())
        {
            TheOnlyAppIs<PostgresBackedLocal>();
        }
    }

    [Collection("marten")]
    public class LocalPostgresqlBackedTransportCompliance : SendingCompliance<LocalPostgresqlBackedFixture>
    {
        public LocalPostgresqlBackedTransportCompliance(LocalPostgresqlBackedFixture fixture) : base(fixture)
        {
        }
    }
}
