using System.Data.Common;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Testing.Marten;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Postgresql
{
    public class extension_registrations : PostgresqlContext
    {
        [Fact]
        public void registrations()
        {
            using (var runtime = JasperHost.For(x =>
                x.Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString)))
            {
                var container = runtime.Get<IContainer>();
                container.Model.HasRegistrationFor<NpgsqlConnection>().ShouldBeTrue();
                container.Model.HasRegistrationFor<DbConnection>().ShouldBeTrue();

                container.Model.For<NpgsqlConnection>().Default.Lifetime.ShouldBe(ServiceLifetime.Scoped);


                container.Model.HasRegistrationFor<IEnvelopePersistence>().ShouldBeTrue();


                runtime.Get<NpgsqlConnection>().ConnectionString.ShouldBe(Servers.PostgresConnectionString);
                runtime.Get<DbConnection>().ShouldBeOfType<NpgsqlConnection>()
                    .ConnectionString.ShouldBe(Servers.PostgresConnectionString);
            }
        }
    }
}
