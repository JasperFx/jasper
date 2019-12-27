using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling
{
    public class RetryNowContinuation : IContinuation
    {
        public static readonly RetryNowContinuation Instance = new RetryNowContinuation();

        private RetryNowContinuation()
        {
        }


        public Task Execute(IMessageContext context, DateTime utcNow)
        {
            return context.Advanced.Retry();
        }

        public override string ToString()
        {
            return "Retry Now";
        }
    }
}
