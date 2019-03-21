using System.Data.Common;
using IntegrationTests;
using Jasper.Messaging.Durability;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Testing.Marten;
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
                x.Settings.PersistMessagesWithPostgresql(Servers.PostgresConnectionString)))
            {
                runtime.Container.Model.HasRegistrationFor<NpgsqlConnection>().ShouldBeTrue();
                runtime.Container.Model.HasRegistrationFor<DbConnection>().ShouldBeTrue();

                runtime.Container.Model.For<NpgsqlConnection>().Default.Lifetime.ShouldBe(ServiceLifetime.Scoped);


                runtime.Container.Model.HasRegistrationFor<IEnvelopePersistence>().ShouldBeTrue();


                runtime.Get<NpgsqlConnection>().ConnectionString.ShouldBe(Servers.PostgresConnectionString);
                runtime.Get<DbConnection>().ShouldBeOfType<NpgsqlConnection>()
                    .ConnectionString.ShouldBe(Servers.PostgresConnectionString);
            }
        }
    }
}
