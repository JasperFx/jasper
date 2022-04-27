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

        public async ValueTask Execute(IExecutionContext execution, DateTime utcNow)
        {
            await execution.RetryExecutionNow();
        }

        public override string ToString()
        {
            return "Retry Now";
        }
    }
}
