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

        public async Task Execute(IChannelCallback channel, Envelope envelope,
            IExecutionContext execution,
            DateTime utcNow)
        {
            try
            {
                execution.Logger.DiscardedEnvelope(envelope);
                await channel.Complete(envelope);
            }
            catch (Exception e)
            {
                execution.Logger.LogException(e);
            }
        }
    }
}
