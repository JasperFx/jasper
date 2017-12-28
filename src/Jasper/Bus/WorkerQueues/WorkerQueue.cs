using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Scheduled;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.WorkerQueues
{
    public class WorkerQueue : IWorkerQueue
    {
        private readonly CompositeMessageLogger _logger;
        private readonly IHandlerPipeline _pipeline;
        private readonly BusSettings _settings;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, ActionBlock<Envelope>> _receivers
            = new Dictionary<string, ActionBlock<Envelope>>();


        public WorkerQueue(CompositeMessageLogger logger, IHandlerPipeline pipeline, BusSettings settings, CancellationToken cancellationToken)
        {
            _logger = logger;
            _pipeline = pipeline;
            _settings = settings;
            _cancellationToken = cancellationToken;

            foreach (var worker in _settings.Workers.AllWorkers)
            {
                AddQueue(worker.Name, worker.Parallelization);
            }

            ScheduledJobs = InMemoryScheduledJobProcessor.ForQueue(this);
        }

        public IScheduledJobProcessor ScheduledJobs { get; }

        public Task Enqueue(Envelope envelope)
        {
            if (envelope.Callback == null) throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Callback must be set before enqueuing the envelope");

            if (envelope.IsPing()) return Task.CompletedTask;

            var receiver = determineReceiver(envelope);

            receiver.Post(envelope);

            return Task.CompletedTask;
        }

        private ActionBlock<Envelope> determineReceiver(Envelope envelope)
        {
            var queueName = envelope.Queue ?? _settings.Workers.WorkerFor(envelope.MessageType);


            var receiver = _receivers.ContainsKey(queueName)
                ? _receivers[queueName]
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
