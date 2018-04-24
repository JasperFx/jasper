using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Messaging.Durability;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests
{
    public class extension_registrations
    {
        [Fact]
        public void registrations()
        {
            using (var runtime = JasperRuntime.For(x =>
                x.Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString)))
            {

                runtime.Container.Model.HasRegistrationFor<SqlConnection>().ShouldBeTrue();
                runtime.Container.Model.HasRegistrationFor<DbConnection>().ShouldBeTrue();

                runtime.Container.Model.For<SqlConnection>().Default.Lifetime.ShouldBe(ServiceLifetime.Scoped);


                runtime.Container.Model.HasRegistrationFor<IEnvelopePersistor>().ShouldBeTrue();



                runtime.Get<SqlConnection>().ConnectionString.ShouldBe(ConnectionSource.ConnectionString);
                runtime.Get<DbConnection>().ShouldBeOfType<SqlConnection>()
                    .ConnectionString.ShouldBe(ConnectionSource.ConnectionString);
            }
        }
    }
}
