using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public class NulloDurableMessagingFactory : IDurableMessagingFactory
    {
        private readonly ITransportLogger _logger;
        private readonly MessagingSettings _settings;

        public NulloDurableMessagingFactory(ITransportLogger logger, MessagingSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation)
        {
            return new LightweightSendingAgent(destination, sender, _logger, _settings);
        }

        public ISendingAgent BuildLocalAgent(Uri destination, IMessagingRoot root)
        {
            return new LoopbackSendingAgent(destination, root.Workers);
        }

        public IListener BuildListener(IListeningAgent agent, IMessagingRoot root)
        {
            return new LightweightListener(root.Workers, _logger, agent);
        }

        public void ClearAllStoredMessages()
        {
            // nothing
        }

        public IScheduledJobProcessor ScheduledJobs { get; set; }

        public Task ScheduleJob(Envelope envelope)
        {
            if (!envelope.ExecutionTime.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "No value for ExecutionTime");
            }

            ScheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
            return Task.CompletedTask;
        }

        public Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            return Task.FromResult<ErrorReport>(null);
        }
    }
}
