using Jasper;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestMessages;

namespace Sender
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Hosting(x =>
            {
                x.ConfigureAppConfiguration((_, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
                })
                .UseUrls("http://*:5060").UseKestrel()
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Error);
                });

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

            Transports.ListenForMessagesFromUriValueInConfig("listener");
            Publish.AllMessagesToUriValueInConfig("receiver");
        }
    }
}
