using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerBackedDurableMessagingFactory : IDurableMessagingFactory
    {
        public MessagingSettings Settings { get; }
        private readonly SqlServerSettings _sqlServerSettings;
        private readonly ITransportLogger _logger;
        private readonly SqlServerEnvelopePersistor _persistor;
        private readonly EnvelopeRetries _retries;

        public SqlServerBackedDurableMessagingFactory(SqlServerSettings sqlServerSettings, ITransportLogger logger, MessagingSettings settings)
        {
            Settings = settings;
            _sqlServerSettings = sqlServerSettings;
            _logger = logger;
            _persistor = new SqlServerEnvelopePersistor(sqlServerSettings);

            _retries = new EnvelopeRetries(_persistor, logger, settings);
        }

        public ISendingAgent BuildSendingAgent(Uri destination, ISender sender, CancellationToken cancellation)
        {
            return new DurableSendingAgent(destination, sender, _logger, Settings, _retries, _persistor);
        }

        public ISendingAgent BuildLocalAgent(Uri destination, IMessagingRoot root)
        {
            return new LocalSendingAgent(destination, root.Workers, _persistor, root.Serialization, _retries, _logger);
        }

        public IListener BuildListener(IListeningAgent agent, IMessagingRoot root)
        {
            return new DurableListener(agent, root.Workers, _logger, Settings, _retries, _persistor);
        }

        public void ClearAllStoredMessages()
        {
            _persistor.ClearAllStoredMessages();
        }

        public Task StoreOutgoing(SqlTransaction tx, IEnumerable<Envelope> envelopes)
        {
            var cmd = SqlServerEnvelopePersistor.BuildOutgoingStorageCommand(envelopes.ToArray(), Settings.UniqueNodeId, _sqlServerSettings);
            cmd.Transaction = tx;
            cmd.Connection = tx.Connection;
            return cmd.ExecuteNonQueryAsync();
        }

        public Task StoreIncoming(SqlTransaction tx, IEnumerable<Envelope> envelopes)
        {
            var cmd = SqlServerEnvelopePersistor.BuildIncomingStorageCommand(envelopes.ToArray(), _sqlServerSettings);
            cmd.Transaction = tx;
            cmd.Connection = tx.Connection;
            return cmd.ExecuteNonQueryAsync();
        }

        public Task ScheduleJob(Envelope envelope)
        {
            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.Status = TransportConstants.Scheduled;

            if (envelope.Message == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message is required");
            }

            if (!envelope.ExecutionTime.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), "No value for ExecutionTime");
            }

            return _persistor.StoreIncoming(envelope);
        }

    }
}
