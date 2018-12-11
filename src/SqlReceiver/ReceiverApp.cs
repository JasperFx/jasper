using Jasper;
using Jasper.Persistence.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SqlReceiver
{
    public class ReceiverApp : JasperRegistry
    {
        public ReceiverApp()
        {
            Hosting.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json").AddEnvironmentVariables();
            });

            Hosting.UseUrls("http://*:5061").UseKestrel();

            Hosting.ConfigureLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);
                x.AddConsole();
            });

            Settings.PersistMessagesWithSqlServer((context, settings) =>
            {
                settings.SchemaName = "receiver";
                settings.ConnectionString = context.Configuration["mssql"];
            });


            Settings.Configure(c => { Transports.ListenForMessagesFrom(c.Configuration["listener"]); });
        }
    }
}
