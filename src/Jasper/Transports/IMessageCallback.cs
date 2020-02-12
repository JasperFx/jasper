using System;
using System.Threading.Tasks;
using Jasper.Persistence.Durability;
using Jasper.Runtime;

namespace Jasper.Transports
{
    public interface IHasDeadLetterQueue
    {
        Task MoveToErrors(Exception exception);
    }

    public interface IHasNativeScheduling
    {
        Task MoveToScheduledUntil(DateTimeOffset time);
    }

    public static class MessageCallbackExtensions
    {
        public static Task MoveToErrors(this IMessageContext context, IMessagingRoot root, Exception exception)
        {
            var envelope = context.Envelope;

            if (envelope.Callback is IHasDeadLetterQueue c) return c.MoveToErrors(exception);

            if (root.Persistence is NulloEnvelopePersistence)
            {
                return Task.CompletedTask;
            }

            // If persistable, persist
            var errorReport = new ErrorReport(envelope, exception);
            return root.Persistence.MoveToDeadLetterStorage(new ErrorReport[] {errorReport});
        }

        public static Task MoveToScheduledUntil(this IMessageContext context, IMessagingRoot root,
            DateTimeOffset time)
        {
            var envelope = context.Envelope;

            if (envelope.Callback is IHasNativeScheduling c) return c.MoveToScheduledUntil(time);

            if (root.Persistence is NulloEnvelopePersistence)
            {
                root.ScheduledJobs.Enqueue(time, envelope);
                return Task.CompletedTask;
            }
            else
            {
                envelope.ExecutionTime = time;
                return root.Persistence.ScheduleJob(envelope);
            }
        }
    }

    public interface IMessageCallback
    {
        /// <summary>
        /// Mark the message as having been successfully received and processed
        /// </summary>
        /// <returns></returns>
        Task Complete();


        /// <summary>
        /// Requeue the message for later processing
        /// </summary>
        /// <returns></returns>
        Task Defer();
    }

    public interface IFullMessageCallback : IMessageCallback, IHasNativeScheduling, IHasDeadLetterQueue
    {

    }
}
