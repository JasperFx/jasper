using System.Collections.Generic;
using IntegrationTests;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Testing.Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Postgresql
{
    public class configuration_extension_methods : PostgresqlContext
    {
        [Fact]
        public void bootstrap_with_configuration()
        {
            var registry = new JasperRegistry();

            registry.Hosting(x => x.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                    {{"connection", Servers.PostgresConnectionString}});
            }));


            registry.Settings.PersistMessagesWithPostgresql((c, s) =>
            {
                s.ConnectionString = c.Configuration["connection"];
            });

            using (var runtime = JasperHost.For(registry))
            {

                runtime.Get<PostgresqlSettings>()
                    .ConnectionString.ShouldBe(Servers.PostgresConnectionString);
            }
        }


        [Fact]
        public void bootstrap_with_connection_string()
        {
            using (var runtime = JasperHost.For(x =>
                x.Settings.PersistMessagesWithPostgresql(Servers.PostgresConnectionString)))
            {

                runtime.Get<PostgresqlSettings>()
                    .ConnectionString.ShouldBe(Servers.PostgresConnectionString);
            }
        }
    }

    // SAMPLE: AppUsingPostgresql
    public class AppUsingPostgresql : JasperRegistry
    {
        public AppUsingPostgresql()
        {
            // If you know the connection string
            Settings.PersistMessagesWithPostgresql("your connection string", "my_app_schema");

            // Or using application configuration
            Settings.PersistMessagesWithPostgresql((context, settings) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    // if so desired, the context argument gives you
                    // access to both the IConfiguration and IHostingEnvironment
                    // of the running application, so you could do
                    // environment specific configuration here
                }

                settings.ConnectionString = context.Configuration["sqlserver"];

                // If your application uses a schema besides "dbo"
                settings.SchemaName = "my_app_schema";
            });
        }
    }

    // ENDSAMPLE
}
