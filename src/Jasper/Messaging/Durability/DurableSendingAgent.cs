using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Durability
{
    public class DurableSendingAgent : SendingAgent
    {
        private readonly ITransportLogger _logger;
        private readonly MessagingSettings _settings;
        private readonly IRetries _persistenceRetries;
        private readonly IEnvelopePersistor _persistor;

        public DurableSendingAgent(Uri destination, ISender sender,
            ITransportLogger logger, MessagingSettings settings, IRetries persistenceRetries,
            IEnvelopePersistor persistor)
            : base(destination, sender, logger, settings, new DurableRetryAgent(sender, settings.Retries, logger, persistor))
        {
            _logger = logger;
            _settings = settings;
            _persistenceRetries = persistenceRetries;

            _persistor = persistor;
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
            envelope.OwnerId = _settings.UniqueNodeId;
            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;
        }

        public override async Task StoreAndForward(Envelope envelope)
        {
            setDefaults(envelope);

            await _persistor.StoreOutgoing(envelope, _settings.UniqueNodeId);

            await EnqueueOutgoing(envelope);
        }

        public override async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            var outgoing = envelopes.ToArray();

            foreach (var envelope in outgoing)
            {
                setDefaults(envelope);
            }

            await _persistor.StoreOutgoing(outgoing, _settings.UniqueNodeId);

            foreach (var envelope in outgoing)
            {
                await _sender.Enqueue(envelope);
            }
        }

        public override async Task Successful(OutgoingMessageBatch outgoing)
        {
            try
            {
                await _persistor.DeleteOutgoingEnvelopes(outgoing.Messages.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogException(e, message:"Error trying to delete outgoing envelopes after a successful batch send");
                foreach (var envelope in outgoing.Messages)
                {
                    _persistenceRetries.DeleteOutgoing(envelope);
                }
            }
        }

        public override async Task Successful(Envelope outgoing)
        {
            try
            {
                await _persistor.DeleteOutgoingEnvelope(outgoing);
            }
            catch (Exception e)
            {
                _logger.LogException(e, message:"Error trying to delete outgoing envelopes after a successful batch send");
                _persistenceRetries.DeleteOutgoing(outgoing);
            }
        }
    }
}
