using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;

namespace Jasper.Runtime.WorkerQueues
{
    public class InlineWorkerQueue : IListeningWorkerQueue
    {
        private readonly IHandlerPipeline _pipeline;
        private readonly IMessageLogger _logger;

        public InlineWorkerQueue(IHandlerPipeline pipeline, IMessageLogger logger)
        {
            _pipeline = pipeline;
            _logger = logger;
        }

        public void Dispose()
        {
            // Nothing
        }

        public IListener Listener { get; set; }

        public async Task Received(Uri uri, Envelope[] messages)
        {
            foreach (var envelope in messages)
            {
                await Received(uri, envelope);
            }
        }

        public async Task Received(Uri uri, Envelope envelope)
        {
            try
            {
                // TODO -- log that this was incoming?
                await _pipeline.Invoke(envelope, Listener);
            }
            catch (Exception e)
            {
                _logger.LogException(e, envelope.Id, "Failure to receive an incoming message");

                try
                {
                    await Listener.Defer(envelope);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, envelope.CorrelationId,"Error when trying to Nack a Rabbit MQ message that failed in the HandlerPipeline");
                }
            }
        }
    }
}
