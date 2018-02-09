using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.ErrorHandling
{
    public class RetryNowContinuation : IContinuation
    {
        public static readonly RetryNowContinuation Instance = new RetryNowContinuation();

        private RetryNowContinuation()
        {
        }


        public Task Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            return context.Retry(envelope);
        }

        public override string ToString()
        {
            return "Retry Now";
        }
    }
}
