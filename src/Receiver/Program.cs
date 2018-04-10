using System;
using System.Threading.Tasks;
using Jasper;
using Jasper.Marten;
using Marten;
using TestMessages;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            // TODO -- listener
            // Marten config
            // file logging
        }
    }

    public static class HomeEndpoint
    {
        public static string Get(JasperRuntime runtime)
        {
            return "Hey, I'm here";
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
