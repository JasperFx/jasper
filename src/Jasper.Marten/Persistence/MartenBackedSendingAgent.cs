using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;
using Jasper.Marten.Persistence.Resiliency;
using Marten;
using Marten.Util;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.VisualBasic;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence
{
    public class MartenBackedSendingAgent : SendingAgent
    {
        private readonly CancellationToken _cancellation;
        private readonly CompositeTransportLogger _logger;
        private readonly IDocumentStore _store;
        private readonly BusSettings _settings;
        private readonly EnvelopeTables _marker;
        private readonly MartenRetries _retries;

        public MartenBackedSendingAgent(Uri destination, IDocumentStore store, ISender sender, CancellationToken cancellation, CompositeTransportLogger logger, BusSettings settings, EnvelopeTables marker, MartenRetries retries)
            : base(destination, sender, logger, settings, new MartenBackedRetryAgent(store, sender, settings.Retries, marker, logger))
        {
            _cancellation = cancellation;
            _logger = logger;
            _store = store;
            _settings = settings;
            _marker = marker;
            _retries = retries;
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

            using (var session = _store.LightweightSession())
            {
                var operation = new StoreOutgoingEnvelope(_marker.Outgoing, envelope, _settings.UniqueNodeId);
                session.QueueOperation(operation);
                await session.SaveChangesAsync(_cancellation);
            }

            await EnqueueOutgoing(envelope);
        }

        public override async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                setDefaults(envelope);
            }

            using (var session = _store.LightweightSession())
            {
                foreach (var envelope in envelopes)
                {
                    var operation = new StoreOutgoingEnvelope(_marker.Outgoing, envelope, _settings.UniqueNodeId);
                    session.QueueOperation(operation);
                }

                await session.SaveChangesAsync(_cancellation);
            }

            foreach (var envelope in envelopes)
            {
                await _sender.Enqueue(envelope);
            }
        }

        public override async Task Successful(OutgoingMessageBatch outgoing)
        {
            try
            {
                using (var conn = _store.Tenancy.Default.CreateConnection())
                {
                    await conn.OpenAsync(_cancellation);

                    await conn.CreateCommand($"delete from {_marker.Outgoing} where id = ANY(:idlist)")
                        .With("idlist", outgoing.Messages.Select(x => x.Id).ToArray(),
                            NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                        .ExecuteNonQueryAsync(_cancellation);
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, message:"Error trying to delete outgoing envelopes after a successful batch send");
                foreach (var envelope in outgoing.Messages)
                {
                    _retries.DeleteOutgoing(envelope);
                }
            }
        }
    }
}
