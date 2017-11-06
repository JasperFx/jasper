using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Delayed;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Internals.Util;

namespace Jasper.Bus.Transports.WorkerQueues
{
    public class WorkerQueue : IWorkerQueue
    {
        private readonly CompositeLogger _logger;
        private readonly IHandlerPipeline _pipeline;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, ActionBlock<Envelope>> _receivers
            = new Dictionary<string, ActionBlock<Envelope>>();


        public WorkerQueue(CompositeLogger logger, IHandlerPipeline pipeline, CancellationToken cancellationToken)
        {
            _logger = logger;
            _pipeline = pipeline;
            _cancellationToken = cancellationToken;

            // TODO -- should this be configurable?
            AddQueue(TransportConstants.Default, 5);
            AddQueue(TransportConstants.Replies, 5);
            AddQueue(TransportConstants.Retries, 5);

            DelayedJobs = InMemoryDelayedJobProcessor.ForQueue(this);
        }

        public IDelayedJobProcessor DelayedJobs { get; }

        public Task Enqueue(Envelope envelope)
        {
            if (envelope.Callback == null) throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Callback must be set before enqueuing the envelope");

            var receiver = determineReceiver(envelope);

            receiver.Post(envelope);

            return Task.CompletedTask;
        }

        private ActionBlock<Envelope> determineReceiver(Envelope envelope)
        {
            // TODO -- will do fancier routing later

            if (envelope.Queue.IsEmpty())
            {
                return _receivers[TransportConstants.Default];
            }


            var receiver = _receivers.ContainsKey(envelope.Queue)
                ? _receivers[envelope.Queue]
                : _receivers[TransportConstants.Default];

            return receiver;
        }

        public int QueuedCount
        {
            get
            {
                return _receivers.Values.ToArray().Sum(x => x.InputCount);
            }
        }

        public void AddQueue(string queueName, int parallelization)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = parallelization,
                CancellationToken = _cancellationToken
            };

            if (!_receivers.ContainsKey(queueName))
            {
                var receiver = new ActionBlock<Envelope>(envelope =>
                {
                    envelope.ContentType = envelope.ContentType ?? "application/json";

                    return _pipeline.Invoke(envelope);
                }, options);

                _receivers.Add(queueName, receiver);
            }
        }

    }
}
