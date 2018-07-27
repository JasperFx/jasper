using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Messaging;
using Jasper.Persistence.Marten;
using Marten;
using Newtonsoft.Json;
using TestMessages;

namespace Sender
{
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
            var writer = new StringWriter();
            runtime.Describe(writer);

            return writer.ToString();
        }

        public static Task post_clear(IDocumentStore store)
        {
            store.Advanced.Clean.DeleteAllDocuments();

            return Task.CompletedTask;
        }

        [MartenTransaction]
        public static async Task post_one(IMessageContext context, IDocumentSession session)
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
        public static async Task post_two(IMessageContext context, IDocumentSession session)
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
        public static async Task post_three(IMessageContext context, IDocumentSession session)
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
        public static async Task post_four(IMessageContext context, IDocumentSession session)
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
}
