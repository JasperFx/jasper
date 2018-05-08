using Jasper;
using Jasper.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TestMessages;

namespace SqlReceiver
{
    public class ReceiverApp : JasperRegistry
    {
        public ReceiverApp()
        {
            Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();

            Hosting.UseUrls("http://*:5061").UseKestrel();

            Hosting.ConfigureLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);
                //x.AddConsole();
            });

            Settings.PersistMessagesWithSqlServer((context, settings) =>
            {
                settings.SchemaName = "receiver";
                settings.ConnectionString = context.Configuration["mssql"];
            });


            Settings.Configure(c =>
            {
                Transports.ListenForMessagesFrom(c.Configuration["listener"]);
            });

        }
    }
}
