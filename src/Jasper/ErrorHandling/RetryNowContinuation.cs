using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.ErrorHandling
{
    public class RetryNowContinuation : IContinuation
    {
        public static readonly RetryNowContinuation Instance = new RetryNowContinuation();

        private RetryNowContinuation()
        {
        }


        public Task Execute(IChannelCallback channel,
            IExecutionContext execution, DateTime utcNow)
        {
            return execution.Root.Pipeline.Invoke(execution.Envelope, channel);
        }

        public override string ToString()
        {
            return "Retry Now";
        }
    }
}
