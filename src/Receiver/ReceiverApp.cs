using Jasper;
using Jasper.Marten;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TestMessages;

namespace Receiver
{
    public class ReceiverApp : JasperRegistry
    {
        public ReceiverApp()
        {
            Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();

            Hosting.UseUrls("http://*:5061").UseKestrel();

            Hosting.ConfigureLogging(x =>
            {
                //x.AddConsole();
                x.SetMinimumLevel(LogLevel.Information);
            });

            Settings.ConfigureMarten((config, options) =>
            {
                options.PLV8Enabled = false;
                options.AutoCreateSchemaObjects = AutoCreate.None;
                options.Connection(config.Configuration["marten"]);
                options.DatabaseSchemaName = "receiver";
                options.Schema.For<SentTrack>();
                options.Schema.For<ReceivedTrack>();
            });

            Include<MartenBackedPersistence>();



            Settings.Configure(c =>
            {
                Transports.ListenForMessagesFrom(c.Configuration["listener"]);
            });

        }
    }
}
