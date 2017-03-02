using System;
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


        public void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow)
        {
            context.Retry(envelope);
        }
    }
}