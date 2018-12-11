using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Invocation;

namespace Jasper.Messaging.ErrorHandling
{
    public class RequeueContinuation : IContinuation
    {
        public static readonly RequeueContinuation Instance = new RequeueContinuation();

        private RequeueContinuation()
        {
        }

        public Task Execute(IMessageContext context, DateTime utcNow)
        {
            var envelope = context.Envelope;
            return envelope.Callback.Requeue(envelope);
        }

        public override string ToString()
        {
            return "Requeue Message Locallyf";
        }
    }
}
