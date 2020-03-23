using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.ErrorHandling
{
    public class NoHandlerContinuation : IContinuation
    {
        private readonly IMissingHandler[] _handlers;

        public NoHandlerContinuation(IMissingHandler[] handlers)
        {
            _handlers = handlers;
        }

        public async Task Execute(IMessagingRoot root, IChannelCallback channel, Envelope envelope,
            IQueuedOutgoingMessages messages,
            DateTime utcNow)
        {
            root.MessageLogger.NoHandlerFor(envelope);

            foreach (var handler in _handlers)
                try
                {
                    await handler.Handle(envelope, root);
                }
                catch (Exception e)
                {
                    root.MessageLogger.LogException(e);
                }

            if (envelope.AckRequested) await root.Acknowledgements.SendAcknowledgement(envelope);

            await channel.Complete(envelope);

            envelope.MarkCompletion(false);

            // These two lines are important to make the message tracking work
            // if there is no handler
            root.MessageLogger.ExecutionFinished(envelope);
            root.MessageLogger.MessageSucceeded(envelope);
        }
    }
}
