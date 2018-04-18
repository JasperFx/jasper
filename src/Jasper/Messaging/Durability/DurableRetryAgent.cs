using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Durability
{
    public class DurableRetryAgent : RetryAgent
    {
        private readonly ITransportLogger _logger;
        private readonly IEnvelopePersistor _persistor;

        public DurableRetryAgent(ISender sender, RetrySettings settings, ITransportLogger logger,
            IEnvelopePersistor persistor) : base(sender, settings)
        {
            _logger = logger;

            _persistor = persistor;
        }

        public IList<Envelope> Queued { get; private set; } = new List<Envelope>();

        public override async Task EnqueueForRetry(OutgoingMessageBatch batch)
        {
            var expiredInQueue = Queued.Where(x => x.IsExpired()).ToArray();
            var expiredInBatch = batch.Messages.Where(x => x.IsExpired()).ToArray();

            var expired = expiredInBatch.Concat(expiredInQueue).ToArray();
            var all = Queued.Where(x => !expiredInQueue.Contains(x))
                .Concat(batch.Messages.Where(x => !expiredInBatch.Contains(x)))
                .ToList();

            var reassigned = new Envelope[0];
            if (all.Count > _settings.MaximumEnvelopeRetryStorage)
                reassigned = all.Skip(_settings.MaximumEnvelopeRetryStorage).ToArray();


            try
            {
                await _persistor.DiscardAndReassignOutgoing(expired, reassigned, TransportConstants.AnyNode);
                _logger.DiscardedExpired(expired);

                Queued = all.Take(_settings.MaximumEnvelopeRetryStorage).ToList();
            }
            catch (Exception e)
            {
                _logger.LogException(e, message: "Failed while trying to enqueue a message batch for retries");


#pragma warning disable 4014
                Task.Delay(100).ContinueWith(async _ => await EnqueueForRetry(batch));
#pragma warning restore 4014
            }
        }

        protected override async Task afterRestarting(ISender sender)
        {
            var expired = Queued.Where(x => x.IsExpired()).ToArray();
            if (expired.Any()) await _persistor.DeleteIncomingEnvelopes(expired);

            var toRetry = Queued.Where(x => !x.IsExpired()).ToArray();
            Queued = new List<Envelope>();

            foreach (var envelope in toRetry) await _sender.Enqueue(envelope);
        }
    }
}
