using Jasper;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestMessages;

namespace Receiver
{
    public class ReceiverApp : JasperRegistry
    {
        public ReceiverApp()
        {
            Hosting(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
                })
                .UseUrls("http://*:5061")
                .UseKestrel()
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Information);
                });
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

            Transports.ListenForMessagesFromUriValueInConfig("listener");
        }
    }
}
