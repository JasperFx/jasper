using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime;

namespace Jasper.ErrorHandling
{
    public class RetryNowContinuation : IContinuation
    {
        public static readonly RetryNowContinuation Instance = new RetryNowContinuation();

        private RetryNowContinuation()
        {
        }

        public Task Execute(IExecutionContext execution, DateTime utcNow)
        {
            return execution.RetryExecutionNow();
        }

        public override string ToString()
        {
            return "Retry Now";
        }
    }
}
