using Jasper.Marten;
using Marten;
using TestMessages;

namespace Sender
{
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