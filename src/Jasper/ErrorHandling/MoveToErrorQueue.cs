using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.ErrorHandling
{
    public class MoveToErrorQueue : IContinuation
    {
        public MoveToErrorQueue(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public async Task Execute(IMessagingRoot root, IChannelCallback channel, Envelope envelope,
            IQueuedOutgoingMessages messages,
            DateTime utcNow)
        {
            envelope.MarkCompletion(false);

            await root.Acknowledgements.SendFailureAcknowledgement(envelope,
                $"Moved message {envelope.Id} to the Error Queue.\n{Exception}");

            if (channel is IHasDeadLetterQueue c)
            {
                await c.MoveToErrors(envelope, Exception);
            }
            else
            {
                // If persistable, persist
                await root.Persistence.MoveToDeadLetterStorage(envelope, Exception);
            }

            root.MessageLogger.MessageFailed(envelope, Exception);
            root.MessageLogger.MovedToErrorQueue(envelope, Exception);


        }

        public override string ToString()
        {
            return "Move to Error Queue";
        }
    }
}
