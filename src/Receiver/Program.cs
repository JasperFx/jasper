using System;
using System.IO;
using System.Threading.Tasks;
using Jasper;
using Jasper.CommandLine;
using Jasper.Marten;
using Jasper.Messaging.Transports.Configuration;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TestMessages;

namespace Receiver
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<ReceiverApp>(args);
        }
    }

    public class ReceiverApp : JasperRegistry
    {
        public ReceiverApp()
        {
            Configuration.AddJsonFile("appsettings.json");

            Hosting.UseUrls("http://*:5061").UseKestrel();

            Hosting.ConfigureLogging(x =>
            {
                x.AddNLog();
                x.SetMinimumLevel(LogLevel.Information);
            });

            Settings.ConfigureMarten((config, options) =>
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Connection(config["marten"]);
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

    public static class HomeEndpoint
    {
        public static string Index(JasperRuntime runtime)
        {
            var writer = new StringWriter();
            runtime.Describe(writer);

            return writer.ToString();
        }

        public static void post_marten_clear(IDocumentStore store)
        {
            store.Advanced.Clean.CompletelyRemoveAll();
        }
    }

    public static class MessageHandler
    {
        [MartenTransaction]
        public static void Handle(Target target, IDocumentSession session)
        {
            session.Store(new ReceivedTrack
            {
                Id = target.Id,
                MessageType = "Target"
            });
        }

        [MartenTransaction]
        public static void Handle(UserCreated created, IDocumentSession session)
        {
            session.Store(new ReceivedTrack
            {
                Id = created.Id,
                MessageType = "UserCreated"
            });
        }

        [MartenTransaction]
        public static PongMessage Handle(PingMessage message, IDocumentSession session)
        {
            session.Store(new ReceivedTrack
            {
                Id = message.Id,
                MessageType = "PingMessage"
            });

            return new PongMessage
            {
                Id = message.Id,
                Name = message.Name
            };
        }
    }
}
