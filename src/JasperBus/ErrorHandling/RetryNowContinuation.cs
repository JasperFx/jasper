using System;
using System.Threading.Tasks;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;

namespace JasperBus.ErrorHandling
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
    }
}