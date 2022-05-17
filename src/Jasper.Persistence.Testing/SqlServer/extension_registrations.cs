using System.Data.Common;
using IntegrationTests;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer;
using Lamar;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer
{
    public class extension_registrations : SqlServerContext
    {
        [Fact]
        public void registrations()
        {
            using var runtime = JasperHost.For(x =>
                x.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString));
            var container = runtime.Get<IContainer>();

            container.Model.HasRegistrationFor<SqlConnection>().ShouldBeTrue();
            container.Model.HasRegistrationFor<DbConnection>().ShouldBeTrue();

            container.Model.For<SqlConnection>().Default.Lifetime.ShouldBe(ServiceLifetime.Scoped);


            container.Model.HasRegistrationFor<IEnvelopePersistence>().ShouldBeTrue();


            runtime.Get<SqlConnection>().ConnectionString.ShouldBe(Servers.SqlServerConnectionString);
            runtime.Get<DbConnection>().ShouldBeOfType<SqlConnection>()
                .ConnectionString.ShouldBe(Servers.SqlServerConnectionString);
        }
    }
}
