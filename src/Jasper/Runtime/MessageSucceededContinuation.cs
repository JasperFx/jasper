using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Transports;

namespace Jasper.Runtime
{
    public class MessageSucceededContinuation : IContinuation
    {
        public static readonly MessageSucceededContinuation Instance = new MessageSucceededContinuation();

        private MessageSucceededContinuation()
        {
        }

        public async Task Execute(IChannelCallback channel, Envelope envelope,
            IExecutionContext execution,
            DateTime utcNow)
        {
            try
            {
                await execution.SendAllQueuedOutgoingMessages();

                await channel.Complete(envelope);

                execution.Logger.MessageSucceeded(envelope);
            }
            catch (Exception ex)
            {
                await execution.SendFailureAcknowledgement(envelope,"Sending cascading message failed: " + ex.Message);

                execution.Logger.LogException(ex, envelope.Id, ex.Message);
                execution.Logger.MessageFailed(envelope, ex);

                await new MoveToErrorQueue(ex).Execute(channel, envelope, execution, utcNow);
            }
        }
    }
}
