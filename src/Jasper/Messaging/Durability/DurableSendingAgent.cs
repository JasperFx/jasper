using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;
using Polly;
using Polly.Retry;

namespace Jasper.Messaging.Durability
{
    public class DurableSendingAgent : SendingAgent
    {
        private readonly ITransportLogger _logger;
        private readonly IEnvelopePersistence _persistence;
        private readonly AsyncRetryPolicy _policy;

        public DurableSendingAgent(ISender sender, AdvancedSettings settings, ITransportLogger logger, IMessageLogger messageLogger,
            IEnvelopePersistence persistence) : base(logger, messageLogger, sender, settings)
        {
            _logger = logger;

            _persistence = persistence;

            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(i => (i*100).Milliseconds()
                    , (e, timeSpan) => {
                        _logger.LogException(e, message:"Failed while trying to enqueue a message batch for retries");
                    });
        }

        public IList<Envelope> Queued { get; private set; } = new List<Envelope>();

        public override Task EnqueueForRetry(OutgoingMessageBatch batch)
        {
            Task execute(CancellationToken c) => enqueueForRetry(batch);

            return _policy.ExecuteAsync(execute, _settings.Cancellation);
        }

        private async Task enqueueForRetry(OutgoingMessageBatch batch)
        {
            if (_settings.Cancellation.IsCancellationRequested) return;

            var expiredInQueue = Queued.Where(x => x.IsExpired()).ToArray();
            var expiredInBatch = batch.Messages.Where(x => x.IsExpired()).ToArray();

            var expired = expiredInBatch.Concat(expiredInQueue).ToArray();
            var all = Queued.Where(x => !expiredInQueue.Contains(x))
                .Concat(batch.Messages.Where(x => !expiredInBatch.Contains(x)))
                .ToList();

            var reassigned = new Envelope[0];
            if (all.Count > base._settings.MaximumEnvelopeRetryStorage)
                reassigned = all.Skip(base._settings.MaximumEnvelopeRetryStorage).ToArray();

            await _persistence.DiscardAndReassignOutgoing(expired, reassigned, TransportConstants.AnyNode);
            _logger.DiscardedExpired(expired);

            Queued = all.Take(base._settings.MaximumEnvelopeRetryStorage).ToList();
        }

        protected override async Task afterRestarting(ISender sender)
        {
            var expired = Queued.Where(x => x.IsExpired()).ToArray();
            if (expired.Any()) await _persistence.DeleteIncomingEnvelopes(expired);

            var toRetry = Queued.Where(x => !x.IsExpired()).ToArray();
            Queued = new List<Envelope>();

            foreach (var envelope in toRetry) await _sender.Enqueue(envelope);
        }

        public override Task Successful(OutgoingMessageBatch outgoing)
        {
            return _policy.ExecuteAsync(c => _persistence.DeleteOutgoing(outgoing.Messages.ToArray()), _settings.Cancellation);
        }

        public override Task Successful(Envelope outgoing)
        {
            return _policy.ExecuteAsync(c => _persistence.DeleteOutgoing(outgoing), _settings.Cancellation);
        }

        public override bool IsDurable { get; } = true;

        protected override async Task storeAndForward(Envelope envelope)
        {
            await _persistence.StoreOutgoing(envelope, _settings.UniqueNodeId);

            await EnqueueOutgoing(envelope);
        }

    }
}
