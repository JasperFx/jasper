using System;
using System.Threading.Tasks;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.ErrorHandling
{
    public class NoHandlerContinuation : IContinuation
    {
        private readonly IMissingHandler[] _handlers;
        private readonly IMessagingRoot _root;

        public NoHandlerContinuation(IMissingHandler[] handlers, IMessagingRoot root)
        {
            _handlers = handlers;
            _root = root;
        }

        public async Task Execute(IExecutionContext execution,
            DateTime utcNow)
        {
            execution.Logger.NoHandlerFor(execution.Envelope);

            foreach (var handler in _handlers)
                try
                {
                    await handler.Handle(execution.Envelope, _root);
                }
                catch (Exception e)
                {
                    execution.Logger.LogException(e);
                }

            if (execution.Envelope.AckRequested) await execution.SendAcknowledgement(execution.Envelope);

            await execution.Complete();

            // These two lines are important to make the message tracking work
            // if there is no handler
            execution.Logger.ExecutionFinished(execution.Envelope);
            execution.Logger.MessageSucceeded(execution.Envelope);
        }
    }
}
