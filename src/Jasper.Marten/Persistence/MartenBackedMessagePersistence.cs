using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;
using Jasper.Conneg;
using Jasper.Marten.Persistence.Resiliency;
using Marten;
using Marten.Util;

namespace Jasper.Marten.Persistence
{
    public class MartenBackedMessagePersistence : IPersistence
    {
        private readonly IDocumentStore _store;
        private readonly CompositeTransportLogger _logger;
        private readonly SerializationGraph _serializers;
        private MartenRetries _retries;

        public MartenBackedMessagePersistence(IDocumentStore store, CompositeTransportLogger logger, BusSettings settings, EnvelopeTables tables, BusMessageSerializationGraph serializers)
        {
            _store = store;
            _logger = logger;
            Settings = settings;
            Tables = tables;
            _serializers = serializers;

            _retries = new MartenRetries(_store, tables, _logger, Settings);
        }

        public BusSettings Settings { get; }

        public EnvelopeTables Tables { get; }

        public ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation)
        {
            return new MartenBackedSendingAgent(destination, _store, sender, cancellation, _logger, Settings, Tables, _retries);
        }

        public ISendingAgent BuildLocalAgent(Uri destination, IWorkerQueue queues)
        {
            return new LocalSendingAgent(destination, queues, _store, Tables, _serializers, _retries, _logger);
        }

        public IListener BuildListener(IListeningAgent agent, IWorkerQueue queues)
        {
            return new MartenBackedListener(agent, queues, _store, _logger, Settings, Tables, _retries);
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

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            using (var session = _store.QuerySession())
            {
                return await session.LoadAsync<ErrorReport>(id);
            }
        }
    }
}
