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

        public async Task Execute(IMessagingRoot root, IChannelCallback channel, Envelope envelope,
            IQueuedOutgoingMessages messages,
            DateTime utcNow)
        {
            try
            {
                root.MessageLogger.DiscardedEnvelope(envelope);
                await channel.Complete(envelope);
            }
            catch (Exception e)
            {
                root.MessageLogger.LogException(e);
            }
        }
    }
}
