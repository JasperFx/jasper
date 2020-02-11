using System;
using System.Threading.Tasks;

namespace Jasper.Runtime
{
    // SAMPLE: IContinuation
    /// <summary>
    /// Represents an action to take after processing a message
    /// </summary>
    public interface IContinuation
    {
        /// <summary>
        /// Post-message handling action
        /// </summary>
        /// <param name="root">A "hub" object giving you access to the Jasper messaging infrastructure</param>
        /// <param name="context">The message context for the just processed message</param>
        /// <param name="utcNow">The current time</param>
        /// <returns></returns>
        Task Execute(IMessagingRoot root, IMessageContext context, DateTime utcNow);
    }

    // ENDSAMPLE
}
