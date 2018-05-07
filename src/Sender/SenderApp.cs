using Jasper;
using Jasper.Marten;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TestMessages;

namespace Sender
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();

            Hosting.UseUrls("http://*:5060").UseKestrel();

            Hosting.ConfigureLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Error);
            });

            Settings.ConfigureMarten((config, options) =>
            {
                options.AutoCreateSchemaObjects = AutoCreate.None;
                options.Connection(config.Configuration["marten"]);
                options.DatabaseSchemaName = "sender";
                options.PLV8Enabled = false;

                options.Schema.For<SentTrack>();
                options.Schema.For<ReceivedTrack>();
            });

            Include<MartenBackedPersistence>();

            Settings.Configure(c =>
            {
                Transports.ListenForMessagesFrom(c.Configuration["listener"]);
                Publish.AllMessagesTo(c.Configuration["receiver"]);
            });

        }
    }
}
