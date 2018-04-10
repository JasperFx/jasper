using System;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.CommandLine;
using Jasper.Marten;
using Jasper.Messaging;
using Jasper.Messaging.Transports.Configuration;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using TestMessages;

namespace Sender
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run<SenderApp>(args);
        }
    }

    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Hosting.UseUrls("http://*:5060").UseKestrel();

            Settings.ConfigureMarten((config, options) =>
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Connection(config["marten"]);
                options.DatabaseSchemaName = "sender";

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

    public class HomeEndpoint
    {
        private static string _json1;
        private static string _json2;


        static HomeEndpoint()
        {
            _json1 = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(HomeEndpoint), "target1.json").ReadAllText();
            _json2 = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(HomeEndpoint), "target2.json").ReadAllText();
        }


        public static string Index(JasperRuntime runtime)
        {
            // TODO -- describe the runtime here?
            return "Hey, I'm the sending application and I'm up and running";
        }

        public static Task post_marten_clear(IDocumentStore store)
        {
            store.Advanced.Clean.CompletelyRemoveAll();

            return Task.CompletedTask;
        }

        [MartenTransaction]
        public static async Task post_marten_one(IMessageContext context, IDocumentSession session)
        {
            await context.EnlistInTransaction(session);

            var target1 = JsonConvert.DeserializeObject<Target>(_json1);
            target1.Id = Guid.NewGuid();

            session.Store(new SentTrack
            {
                Id = target1.Id,
                MessageType = "Target"
            });

            await context.Send(target1);
        }

        [MartenTransaction]
        public static async Task post_marten_two(IMessageContext context, IDocumentSession session)
        {
            await context.EnlistInTransaction(session);

            var target2 = JsonConvert.DeserializeObject<Target>(_json2);
            target2.Id = Guid.NewGuid();

            session.Store(new SentTrack
            {
                Id = target2.Id,
                MessageType = "Target"
            });

            await context.Send(target2);
        }

        [MartenTransaction]
        public static async Task post_marten_three(IMessageContext context, IDocumentSession session)
        {
            await context.EnlistInTransaction(session);

            var ping = new PingMessage
            {
                Name = "Han Solo",
                Id = Guid.NewGuid()
            };

            session.Store(new SentTrack
            {
                Id = ping.Id,
                MessageType = "PingMessage"
            });

            await context.SendAndExpectResponseFor<PongMessage>(ping);
        }

        [MartenTransaction]
        public static async Task post_marten_four(IMessageContext context, IDocumentSession session)
        {
            await context.EnlistInTransaction(session);

            var created = new UserCreated
            {
                Id = Guid.NewGuid(),
                UserId = "Chewbacca"
            };

            session.Store(new SentTrack
            {
                Id = created.Id,
                MessageType = "UserCreated"
            });

            await context.Send(created);
        }
    }

    public class PongHandler
    {
        [MartenTransaction]
        public void Handle(PongMessage message, IDocumentSession session)
        {
            session.Store(new ReceivedTrack
            {
                Id = message.Id,
                MessageType = "PongMessage"
            });
        }
    }
}
