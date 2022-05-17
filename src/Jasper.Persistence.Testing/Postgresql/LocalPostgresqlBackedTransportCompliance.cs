using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Postgresql;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.Persistence.Testing.Postgresql
{


    public class LocalPostgresqlBackedFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public LocalPostgresqlBackedFixture() : base("local://one/durable".ToUri())
        {

        }

        public Task InitializeAsync()
        {
            return TheOnlyAppIs(opts =>
            {
                opts.PersistMessagesWithPostgresql(Servers.PostgresConnectionString);
            });
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }
    }

    [Collection("marten")]
    public class LocalPostgresqlBackedTransportCompliance : SendingCompliance<LocalPostgresqlBackedFixture>
    {

    }
}
