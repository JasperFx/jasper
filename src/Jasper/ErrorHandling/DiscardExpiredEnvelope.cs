using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.ErrorHandling
{
    public class DiscardExpiredEnvelope : IContinuation
    {
        public static readonly DiscardExpiredEnvelope Instance = new DiscardExpiredEnvelope();

        private DiscardExpiredEnvelope(){}

        public async Task Execute(IExecutionContext execution,
            DateTime utcNow)
        {
            try
            {
                execution.Logger.DiscardedEnvelope(execution.Envelope);
                await execution.Complete();
            }
            catch (Exception? e)
            {
                execution.Logger.LogException(e);
            }
        }
    }
}
