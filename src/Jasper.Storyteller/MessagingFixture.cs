using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using StoryTeller;

namespace Jasper.Storyteller
{
    public abstract class MessagingFixture : Fixture
    {
        /// <summary>
        /// The service bus for the currently running application
        /// </summary>
        protected IMessageContext Bus => Context.Service<IMessageContext>();

        protected MessageHistory History => Context.Service<MessageHistory>();

        /// <summary>
        /// Send a message and wait for all detected activity within the bus
        /// to complete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected Task SendMessageAndWaitForCompletion(object message)
        {
            return History.WatchAsync(() => Bus.Send(message));
        }
    }
}
