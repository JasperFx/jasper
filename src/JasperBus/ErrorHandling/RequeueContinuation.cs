using System;
using System.Threading.Tasks;
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

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            // TODO -- should the callback stuff be async too?
            envelope.Callback.Requeue(envelope);
            return Task.CompletedTask;
        }
    }
}