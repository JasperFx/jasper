using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public class RequeueContinuation : IContinuation
    {
        public static readonly RequeueContinuation Instance = new RequeueContinuation();

        private RequeueContinuation()
        {
        }

        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            return envelope.Callback.Requeue(envelope);
        }
    }
}
