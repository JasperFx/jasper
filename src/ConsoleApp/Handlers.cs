using System.Threading.Tasks;
using StoryTeller.Results;
using TestingSupport.Compliance;
using TestMessages;

namespace MyApp
{
    public class PingHandler
    {
        public void Ping(Envelope envelope, PingMessage message)
        {

        }
    }

    public class PongHandler
    {
        public Task Handle(PongMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
