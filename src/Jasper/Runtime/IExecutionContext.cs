using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Transports;

namespace Jasper.Runtime
{
    public interface IExecutionContext : IAcknowledgementSender
    {
        Task SendAllQueuedOutgoingMessages();
        IMessageLogger Logger { get; }

        IMessagePublisher NewPublisher();

        Envelope Envelope { get; }

        /// <summary>
        /// Mark the message as having been successfully received and processed
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task Complete();

        /// <summary>
        /// Requeue the message for later processing
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        Task Defer();

        Task ReSchedule(DateTime scheduledTime);

        Task MoveToDeadLetterQueue(Exception exception);

        Task RetryExecutionNow();
    }
}
