using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;
using Marten;
using Marten.Util;

namespace Jasper.Marten.Persistence
{
    public class MartenBackedDurableMessagingFactory : IDurableMessagingFactory
    {
        private readonly IDocumentStore _store;
        private readonly ITransportLogger _logger;
        private readonly EnvelopeRetries _retries;
        private MartenEnvelopePersistor _persistor;

        public MartenBackedDurableMessagingFactory(IDocumentStore store, ITransportLogger logger,
            MessagingSettings settings, EnvelopeTables tables)
        {
            _store = store;
            _logger = logger;
            Settings = settings;
            Tables = tables;

            _persistor = new MartenEnvelopePersistor(_store, tables);

            _retries = new EnvelopeRetries(new MartenEnvelopePersistor(_store, tables), _logger, Settings);
        }

        public MessagingSettings Settings { get; }

        public EnvelopeTables Tables { get; }

        public ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation)
        {
            _store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));
            return new MartenBackedSendingAgent(destination, _store, sender, cancellation, _logger, Settings, Tables, _retries);
        }

        public ISendingAgent BuildLocalAgent(Uri destination, IMessagingRoot root)
        {
            _store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));
            return new LocalSendingAgent(destination, root.Workers, _persistor, root.Serialization, _retries);
        }

        public IListener BuildListener(IListeningAgent agent, IMessagingRoot root)
        {
            _store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));
            return new MartenBackedListener(agent, root.Workers, _store, _logger, Settings, Tables, _retries);
        }

        public void ClearAllStoredMessages()
        {
            using (var conn = _store.Tenancy.Default.CreateConnection())
            {
                conn.Open();

                conn.CreateCommand().Sql($"delete from {Tables.Incoming};delete from {Tables.Outgoing}")
                    .ExecuteNonQuery();

            }
        }

        public async Task ScheduleJob(Envelope envelope)
        {
            envelope.Status = TransportConstants.Scheduled;

            if (envelope.Message == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message is required");
            }

            if (!envelope.ExecutionTime.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "No value for ExecutionTime");
            }

            using (var session = _store.LightweightSession())
            {
                session.StoreIncoming(Tables, envelope);
                await session.SaveChangesAsync();
            }
        }


    }
}
