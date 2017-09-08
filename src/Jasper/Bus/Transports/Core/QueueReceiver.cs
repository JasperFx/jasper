using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.Core
{
    public class QueueReceiver : IDisposable
    {
        private readonly IHandlerPipeline _pipeline;
        private readonly IQueueProvider _provider;
        private readonly CancellationToken _cancellationToken;
        private readonly ActionBlock<Envelope> _block;
        public string QueueName { get; }

        public QueueReceiver(IHandlerPipeline pipeline, string queueName, int maximumParallelization, IQueueProvider provider, CancellationToken cancellationToken)
        {
            _pipeline = pipeline;
            _provider = provider;
            _cancellationToken = cancellationToken;
            QueueName = queueName;


            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maximumParallelization,
                CancellationToken = cancellationToken
            };


            _block = new ActionBlock<Envelope>(receive, options);
        }

        private Task receive(Envelope envelope)
        {
            envelope.Callback = _provider.BuildCallback(envelope, this);
            envelope.ContentType = envelope.ContentType ?? "application/json";

            return _pipeline.Invoke(envelope);
        }

        public void Enqueue(Envelope message)
        {
            _block.Post(message);
        }

        public void Dispose()
        {
            _block.Complete();
        }
    }


}
