using System;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus.ErrorHandling
{
    public class RequeueContinuation : IContinuation
    {
        public static readonly RequeueContinuation Instance = new RequeueContinuation();

        private RequeueContinuation()
        {
        }

        public void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            envelope.Callback.Requeue();
        }
    }
}