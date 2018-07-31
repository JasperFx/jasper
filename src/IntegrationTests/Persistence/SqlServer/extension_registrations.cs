using System.Data.Common;
using System.Data.SqlClient;
using Jasper;
using Jasper.Messaging.Durability;
using Jasper.Persistence.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Servers;
using Servers.Docker;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer
{
    public class extension_registrations : SqlServerContext
    {
        [Fact]
        public void registrations()
        {
            using (var runtime = JasperRuntime.For(x =>
                x.Settings.PersistMessagesWithSqlServer(SqlServerContainer.ConnectionString)))
            {

                runtime.Container.Model.HasRegistrationFor<SqlConnection>().ShouldBeTrue();
                runtime.Container.Model.HasRegistrationFor<DbConnection>().ShouldBeTrue();

                runtime.Container.Model.For<SqlConnection>().Default.Lifetime.ShouldBe(ServiceLifetime.Scoped);


                runtime.Container.Model.HasRegistrationFor<IEnvelopePersistor>().ShouldBeTrue();



                runtime.Get<SqlConnection>().ConnectionString.ShouldBe(SqlServerContainer.ConnectionString);
                runtime.Get<DbConnection>().ShouldBeOfType<SqlConnection>()
                    .ConnectionString.ShouldBe(SqlServerContainer.ConnectionString);
            }
        }

        public extension_registrations(DockerFixture<SqlServerContainer> fixture) : base(fixture)
        {
        }
    }
}
