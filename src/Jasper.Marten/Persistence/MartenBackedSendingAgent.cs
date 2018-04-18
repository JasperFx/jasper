using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;
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
        private readonly ITransportLogger _logger;
        private readonly IDocumentStore _store;
        private readonly MessagingSettings _settings;
        private readonly EnvelopeTables _marker;
        private readonly IRetries _retries;

        public MartenBackedSendingAgent(Uri destination, IDocumentStore store, ISender sender, CancellationToken cancellation, ITransportLogger logger, MessagingSettings settings, EnvelopeTables marker, IRetries retries)
            : base(destination, sender, logger, settings, new DurableRetryAgent(sender, settings.Retries, logger, new MartenEnvelopePersistor(store, marker)))
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

        public override async Task Successful(Envelope outgoing)
        {
            try
            {
                using (var conn = _store.Tenancy.Default.CreateConnection())
                {
                    await conn.OpenAsync(_cancellation);

                    await conn.CreateCommand($"delete from {_marker.Outgoing} where id = :id")
                        .With("id", outgoing.Id, NpgsqlDbType.Uuid)
                        .ExecuteNonQueryAsync(_cancellation);
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e, message:"Error trying to delete outgoing envelopes after a successful batch send");
                _retries.DeleteOutgoing(outgoing);
            }
        }
    }
}
