using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Scheduled;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Bus.Transports
{
    public class NulloPersistence : IPersistence
    {
        private readonly CompositeTransportLogger _logger;
        private readonly BusSettings _settings;
        private InMemoryScheduledJobProcessor _scheduledJobs;

        public NulloPersistence(CompositeTransportLogger logger, BusSettings settings)
        {
            _logger = logger;
            _settings = settings;
            _scheduledJobs = new InMemoryScheduledJobProcessor();
        }

        public ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation)
        {
            return new LightweightSendingAgent(destination, sender, _logger, _settings);
        }

        public ISendingAgent BuildLocalAgent(Uri destination, IWorkerQueue queues)
        {
            return new LoopbackSendingAgent(destination, queues);
        }

        public IListener BuildListener(IListeningAgent agent, IWorkerQueue queues)
        {
            return new LightweightListener(queues, _logger, agent);
        }

        public void ClearAllStoredMessages()
        {
            // nothing
        }

        public Task ScheduleJob(Envelope envelope)
        {
            if (!envelope.ExecutionTime.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "No value for ExecutionTime");
            }

            _scheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
            return Task.CompletedTask;
        }

        public Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            return Task.FromResult<ErrorReport>(null);
        }
    }
}
