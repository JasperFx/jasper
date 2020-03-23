using System;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;
using Jasper.Runtime;

namespace Jasper.Transports
{
    /// <summary>
    /// Marks an IChannelCallback as supporting a native dead letter queue
    /// functionality
    /// </summary>
    public interface IHasDeadLetterQueue
    {
        Task MoveToErrors(Envelope envelope, Exception exception);
    }

    /// <summary>
    /// Marks an IChannelCallback as supporting native scheduled send
    /// </summary>
    public interface IHasNativeScheduling
    {
        /// <summary>
        /// Move the current message represented by the envelope to a
        /// scheduled delivery
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Task MoveToScheduledUntil(Envelope envelope, DateTimeOffset time);
    }


    public interface IChannelCallback
    {
        /// <summary>
        /// Mark the message as having been successfully received and processed
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task Complete(Envelope envelope);


        /// <summary>
        /// Requeue the message for later processing
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task Defer(Envelope envelope);
    }

}
