using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Transports;

namespace Jasper.Runtime
{
    public class MessageSucceededContinuation : IContinuation
    {
        public static readonly MessageSucceededContinuation Instance = new MessageSucceededContinuation();

        private MessageSucceededContinuation()
        {
        }

        public async Task Execute(IMessagingRoot root, IChannelCallback channel, Envelope envelope,
            IQueuedOutgoingMessages messages,
            DateTime utcNow)
        {
            try
            {
                await messages.SendAllQueuedOutgoingMessages();

                await channel.Complete(envelope);

                root.MessageLogger.MessageSucceeded(envelope);
            }
            catch (Exception ex)
            {
                await root.Acknowledgements.SendFailureAcknowledgement(envelope,"Sending cascading message failed: " + ex.Message);

                root.MessageLogger.LogException(ex, envelope.Id, ex.Message);
                root.MessageLogger.MessageFailed(envelope, ex);

                await new MoveToErrorQueue(ex).Execute(root, channel, envelope, messages, utcNow);
            }
        }
    }
}
