using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
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
        private readonly JasperOptions _options;
        private readonly RetryPolicy _policy;

        public DurableSendingAgent(Uri destination, ISender sender,
            ITransportLogger logger, JasperOptions options,
            IEnvelopePersistence persistence)
            : base(destination, sender, logger, options,
                new DurableRetryAgent(sender, options, logger, persistence))
        {
            _logger = logger;
            _options = options;

            _persistence = persistence;

            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(i => (i*100).Milliseconds()
                , (e, timeSpan) => {
                    _logger.LogException(e);
                });
        }

        public override bool IsDurable => true;

        public override Task EnqueueOutgoing(Envelope envelope)
        {
            setDefaults(envelope);

            return _sender.Enqueue(envelope);
        }

        private void setDefaults(Envelope envelope)
        {
            envelope.EnsureData();
            envelope.OwnerId = _options.UniqueNodeId;
            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;
        }

        public override async Task StoreAndForward(Envelope envelope)
        {
            setDefaults(envelope);

            await _persistence.StoreOutgoing(envelope, _options.UniqueNodeId);

            await EnqueueOutgoing(envelope);
        }

        public override async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            var outgoing = envelopes.ToArray();

            foreach (var envelope in outgoing) setDefaults(envelope);

            await _persistence.StoreOutgoing(outgoing, _options.UniqueNodeId);

            foreach (var envelope in outgoing) await _sender.Enqueue(envelope);
        }

        public override Task Successful(OutgoingMessageBatch outgoing)
        {
            return _policy.ExecuteAsync(c => _persistence.DeleteOutgoing(outgoing.Messages.ToArray()), _options.Cancellation);
        }

        public override Task Successful(Envelope outgoing)
        {
            return _policy.ExecuteAsync(c => _persistence.DeleteOutgoing(outgoing), _options.Cancellation);
        }
    }
}
