using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;

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
        /// <param name="root"></param>
        /// <param name="channel"></param>
        /// <param name="envelope"></param>
        /// <param name="messages"></param>
        /// <param name="utcNow">The current time</param>
        /// <returns></returns>
        Task Execute(IMessagingRoot root, IChannelCallback channel, Envelope envelope,
            IQueuedOutgoingMessages messages, DateTime utcNow);
    }

    // ENDSAMPLE
}
