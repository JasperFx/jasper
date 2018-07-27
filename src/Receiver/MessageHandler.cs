using Jasper.Persistence.Marten;
using Marten;
using TestMessages;

namespace Receiver
{
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
