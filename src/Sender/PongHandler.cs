using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Marten;
using TestMessages;

namespace Sender
{
    public class PongHandler
    {
        [Transaction]
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
