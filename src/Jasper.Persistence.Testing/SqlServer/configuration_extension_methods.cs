using System.Collections.Generic;
using System.Linq;
using IntegrationTests;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
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
                .UseJasper((context, options) =>
                {
                    options.Extensions.PersistMessagesWithSqlServer(context.Configuration["connection"]);
                });


            using var host = builder.Build();
            host.Services.GetRequiredService<SqlServerSettings>()
                .ConnectionString.ShouldBe(Servers.SqlServerConnectionString);

            var databases = host.Services.GetServices<Weasel.Core.Migrations.IDatabase>();
            databases.OfType<SqlServerEnvelopePersistence>()
                .Count().ShouldBe(1);
        }


        [Fact]
        public void bootstrap_with_connection_string()
        {
            using var runtime = JasperHost.For(x =>
                x.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString));
            runtime.Get<SqlServerSettings>()
                .ConnectionString.ShouldBe(Servers.SqlServerConnectionString);
        }
    }

}
