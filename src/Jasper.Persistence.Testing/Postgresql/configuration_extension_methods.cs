using System.Collections.Generic;
using IntegrationTests;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.Testing.Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Postgresql
{
    public class configuration_extension_methods : PostgresqlContext
    {
        [Fact]
        public void bootstrap_with_configuration()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string>
                            {{"connection", Servers.PostgresConnectionString}});
                    })
                .UseJasper((context, x) =>
                {
                    x.PersistMessagesWithPostgresql(context.Configuration["connection"]);
                });


            using (var host = builder.Build())
            {

                host.Services.GetRequiredService<PostgresqlSettings>()
                    .ConnectionString.ShouldBe(Servers.PostgresConnectionString);
            }
        }


        [Fact]
        public void bootstrap_with_connection_string()
        {
            using (var runtime = JasperHost.For(x =>
                x.Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString)))
            {

                runtime.Get<PostgresqlSettings>()
                    .ConnectionString.ShouldBe(Servers.PostgresConnectionString);
            }
        }
    }

    // SAMPLE: AppUsingPostgresql
    public class AppUsingPostgresql : JasperOptions
    {
        public AppUsingPostgresql()
        {
            // If you know the connection string
            Extensions.PersistMessagesWithPostgresql("your connection string", "my_app_schema");


        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Or if you need access to the application configuration and hosting
            // But this time you may need to work directly with the PostgresqlBackedPersistence
            Extensions.Include<PostgresqlBackedPersistence>(ext =>
            {
                if (hosting.IsDevelopment())
                {
                    // if so desired, the context argument gives you
                    // access to both the IConfiguration and IHostingEnvironment
                    // of the running application, so you could do
                    // environment specific configuration here
                }

                ext.Settings.ConnectionString = config["postgresql"];

                // If your application uses a schema besides "dbo"
                ext.Settings.SchemaName = "my_app_schema";
            });

        }
    }

    // ENDSAMPLE
}
