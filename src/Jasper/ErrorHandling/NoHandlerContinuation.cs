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

        public async Task Execute(IChannelCallback channel, Envelope envelope,
            IExecutionContext execution,
            DateTime utcNow)
        {
            execution.Logger.NoHandlerFor(envelope);

            foreach (var handler in _handlers)
                try
                {
                    await handler.Handle(envelope, execution.Root);
                }
                catch (Exception e)
                {
                    execution.Logger.LogException(e);
                }

            if (envelope.AckRequested) await execution.SendAcknowledgement(envelope);

            await channel.Complete(envelope);

            // These two lines are important to make the message tracking work
            // if there is no handler
            execution.Logger.ExecutionFinished(envelope);
            execution.Logger.MessageSucceeded(envelope);
        }
    }
}
