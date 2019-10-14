using System.Collections.Generic;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer
{
    public class configuration_extension_methods : SqlServerContext
    {
        [Fact]
        public void bootstrap_with_configuration()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                        {{"connection", Servers.SqlServerConnectionString}});
                })
                .UseJasper(registry =>
                {
                    registry.Settings.PersistMessagesWithSqlServer((c, s) =>
                    {
                        s.ConnectionString = c.Configuration["connection"];
                    });
                });



            using (var host = builder.Build())
            {

                host.Services.GetRequiredService<SqlServerSettings>()
                    .ConnectionString.ShouldBe(Servers.SqlServerConnectionString);
            }
        }


        [Fact]
        public void bootstrap_with_connection_string()
        {
            using (var runtime = JasperHost.For(x =>
                x.Settings.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString)))
            {

                runtime.Get<SqlServerSettings>()
                    .ConnectionString.ShouldBe(Servers.SqlServerConnectionString);
            }
        }
    }

    // SAMPLE: AppUsingSqlServer
    public class AppUsingSqlServer : JasperRegistry
    {
        public AppUsingSqlServer()
        {
            // If you know the connection string
            Settings.PersistMessagesWithSqlServer("your connection string", "my_app_schema");

            // Or using application configuration
            Settings.PersistMessagesWithSqlServer((context, settings) =>
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

                // If you're using a database principal that is NOT "dbo":
                settings.DatabasePrincipal = "not_dbo";
            });
        }
    }

    // ENDSAMPLE
}
