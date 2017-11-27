using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;
using Jasper.Marten.Persistence.Resiliency;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class MartenBackedSendingAgent : SendingAgent
    {
        private readonly CancellationToken _cancellation;
        private readonly IDocumentStore _store;

        public MartenBackedSendingAgent(Uri destination, IDocumentStore store, ISender sender, CancellationToken cancellation, CompositeLogger logger, BusSettings settings, OwnershipMarker marker)
            : base(destination, sender, logger, settings, new MartenBackedRetryAgent(store, sender, settings.Retries, marker))
        {
            _cancellation = cancellation;
            _store = store;
        }

        public override Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.EnsureData();

            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;

            return _sender.Enqueue(envelope);
        }

        public override async Task StoreAndForward(Envelope envelope)
        {
            envelope.EnsureData();

            using (var session = _store.LightweightSession())
            {
                session.Store(envelope);
                await session.SaveChangesAsync(_cancellation);
            }

            await EnqueueOutgoing(envelope);
        }

        public override async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                envelope.EnsureData();
            }

            using (var session = _store.LightweightSession())
            {
                session.Store(envelopes.ToArray());
                await session.SaveChangesAsync(_cancellation);
            }

            foreach (var envelope in envelopes)
            {
                await EnqueueOutgoing(envelope);
            }
        }

        public override void Successful(OutgoingMessageBatch outgoing)
        {
            // TODO -- retries?
            using (var session = _store.LightweightSession())
            {
                foreach (var message in outgoing.Messages)
                {
                    session.Delete(message);
                }

                session.SaveChanges();
            }
        }
    }
}
