using System;
using System.Threading.Tasks;
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

        public async Task Execute(IMessagingRoot root, IMessageContext context, DateTime utcNow)
        {
            var envelope = context.Envelope;


            await context.Advanced.SendFailureAcknowledgement(
                $"Moved message {envelope.Id} to the Error Queue.\n{Exception}");

            await envelope.MoveToErrors(root, Exception);

            context.Advanced.Logger.MessageFailed(envelope, Exception);
            context.Advanced.Logger.MovedToErrorQueue(envelope, Exception);
        }

        public override string ToString()
        {
            return "Move to Error Queue";
        }
    }
}
