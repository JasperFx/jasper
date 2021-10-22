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

        public async Task Execute(IChannelCallback channel,
            IExecutionContext execution,
            DateTime utcNow)
        {
            try
            {
                await execution.SendAllQueuedOutgoingMessages();

                await channel.Complete(execution.Envelope);

                execution.Logger.MessageSucceeded(execution.Envelope);
            }
            catch (Exception ex)
            {
                await execution.SendFailureAcknowledgement(execution.Envelope,"Sending cascading message failed: " + ex.Message);

                execution.Logger.LogException(ex, execution.Envelope.Id, ex.Message);
                execution.Logger.MessageFailed(execution.Envelope, ex);

                await new MoveToErrorQueue(ex).Execute(channel, execution, utcNow);
            }
        }
    }
}
