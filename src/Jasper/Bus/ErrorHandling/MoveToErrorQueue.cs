using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public class MoveToErrorQueue : IContinuation
    {
        public MoveToErrorQueue(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }

        public async Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            await context.SendFailureAcknowledgement(envelope, $"Moved message {envelope.Id} to the Error Queue.\n{Exception}");

            await envelope.Callback.MoveToErrors(envelope, Exception);

            context.Logger.MovedToErrorQueue(envelope, Exception);

        }

        public override string ToString()
        {
            return $"Move to Error Queue";
        }
    }
}
