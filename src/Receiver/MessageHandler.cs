using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Marten;
using TestMessages;

namespace Receiver
{
    [Transactional]
    public static class MessageHandler
    {
        public static void Handle(Target target, IDocumentSession session)
        {
            session.Store(new ReceivedTrack
            {
                Id = target.Id,
                MessageType = "Target"
            });
        }

        public static void Handle(UserCreated created, IDocumentSession session)
        {
            session.Store(new ReceivedTrack
            {
                Id = created.Id,
                MessageType = "UserCreated"
            });
        }

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
