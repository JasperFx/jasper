using System.Collections.Generic;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Persistence;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests
{
    public class configuration_extension_methods
    {
        [Fact]
        public void bootstrap_with_connection_string()
        {
            using (var runtime = JasperRuntime.For(x =>
                x.Settings.PersistMessagesWithSqlServer(ConnectionSource.ConnectionString)))
            {
                runtime.Container.Model.DefaultTypeFor<IDurableMessagingFactory>()
                    .ShouldBe(typeof(SqlServerBackedDurableMessagingFactory));

                runtime.Get<SqlServerSettings>()
                    .ConnectionString.ShouldBe(ConnectionSource.ConnectionString);
            }
        }

        [Fact]
        public void bootstrap_with_configuration()
        {
            var registry = new JasperRegistry();
            registry.Configuration.AddInMemoryCollection(new Dictionary<string, string> {{"connection", ConnectionSource.ConnectionString}});

            registry.Settings.PersistMessagesWithSqlServer((c, s) =>
                {
                    s.ConnectionString = c.Configuration["connection"];
                });

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Container.Model.DefaultTypeFor<IDurableMessagingFactory>()
                    .ShouldBe(typeof(SqlServerBackedDurableMessagingFactory));

                runtime.Get<SqlServerSettings>()
                    .ConnectionString.ShouldBe(ConnectionSource.ConnectionString);
            }
        }
    }
}
