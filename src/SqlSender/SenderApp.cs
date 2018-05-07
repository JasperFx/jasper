using Jasper;
using Jasper.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace SqlSender
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();

            Hosting.UseUrls("http://*:5060").UseKestrel();

            Hosting.ConfigureLogging(x =>
            {
                x.AddNLog();
                x.SetMinimumLevel(LogLevel.Information);
            });

            Settings.PersistMessagesWithSqlServer((context, settings) =>
            {
                settings.ConnectionString = context.Configuration["mssql"];
                settings.SchemaName = "sender";
            });


            Settings.Configure(c =>
            {
                Transports.ListenForMessagesFrom(c.Configuration["listener"]);
                Publish.AllMessagesTo(c.Configuration["receiver"]);
            });

            Hosting.ConfigureLogging(x => x.AddConsole());
        }
    }
}
