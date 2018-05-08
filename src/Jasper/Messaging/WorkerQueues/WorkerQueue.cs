using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper.Messaging.WorkerQueues
{
    public class WorkerQueue : IWorkerQueue
    {
        private readonly IMessageLogger _logger;
        private readonly MessagingSettings _settings;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, ActionBlock<Envelope>> _receivers
            = new Dictionary<string, ActionBlock<Envelope>>();


        public WorkerQueue(IMessageLogger logger, IHandlerPipeline pipeline, MessagingSettings settings)
        {
            _logger = logger;
            Pipeline = pipeline;
            _settings = settings;
            _cancellationToken = _settings.Cancellation;

            foreach (var worker in _settings.Workers.AllWorkers)
            {
                AddQueue(worker.Name, worker.Parallelization);
            }

            ScheduledJobs = new InMemoryScheduledJobProcessor(this);
        }

        public IScheduledJobProcessor ScheduledJobs { get; }

        // Hate this, but leave it here
        internal IHandlerPipeline Pipeline { get; }

        public Task Enqueue(Envelope envelope)
        {
            if (envelope.Callback == null) throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Callback must be set before enqueuing the envelope");

            if (envelope.IsPing()) return Task.CompletedTask;

            var receiver = determineReceiver(envelope);

            return receiver.SendAsync(envelope, _cancellationToken);

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

                    return Pipeline.Invoke(envelope);
                }, options);

                _receivers.Add(queueName, receiver);
            }
        }

    }
}
