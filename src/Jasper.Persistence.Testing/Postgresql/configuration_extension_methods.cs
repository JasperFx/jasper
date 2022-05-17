using System.Collections.Generic;
using System.Linq;
using IntegrationTests;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.Testing.Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Weasel.Core.Migrations;
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


            using var host = builder.Build();
            host.Services.GetRequiredService<PostgresqlSettings>()
                .ConnectionString.ShouldBe(Servers.PostgresConnectionString);

            host.Services.GetServices<IDatabase>().OfType<PostgresqlEnvelopePersistence>()
                .Count().ShouldBe(1);
        }


        [Fact]
        public void bootstrap_with_connection_string()
        {
            using var runtime = JasperHost.For(x =>
                x.PersistMessagesWithPostgresql(Servers.PostgresConnectionString));
            runtime.Get<PostgresqlSettings>()
                .ConnectionString.ShouldBe(Servers.PostgresConnectionString);
        }
    }


}
