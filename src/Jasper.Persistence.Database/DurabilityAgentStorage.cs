using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Persistence.Database
{
    public abstract class DurabilityAgentStorage : DataAccessor, IDurabilityAgentStorage
    {
        private readonly DurableStorageSession _session;
        private readonly string _findReadyToExecuteJobs;
        private readonly CancellationToken _cancellation;

        protected DurabilityAgentStorage(DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            var transaction = new DurableStorageSession(databaseSettings, settings.Cancellation);

            _session = transaction;
            Session = transaction;

            Nodes = new DurableNodes(transaction, databaseSettings, settings.Cancellation);

            // ReSharper disable once VirtualMemberCallInConstructor
            Incoming = buildDurableIncoming(transaction, databaseSettings, settings);

            // ReSharper disable once VirtualMemberCallInConstructor
            Outgoing = buildDurableOutgoing(transaction, databaseSettings, settings);

            _findReadyToExecuteJobs =
                $"select body from {databaseSettings.SchemaName}.{IncomingTable} where status = '{EnvelopeStatus.Scheduled}' and execution_time <= @time";

            _cancellation = settings.Cancellation;
        }

        protected abstract IDurableOutgoing buildDurableOutgoing(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings, AdvancedSettings settings);

        protected abstract IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings, AdvancedSettings settings);

        public IDurableStorageSession Session { get; }
        public IDurableNodes Nodes { get; }
        public IDurableIncoming Incoming { get; }
        public IDurableOutgoing Outgoing { get; }

        public Task<Envelope[]> LoadScheduledToExecute(DateTimeOffset utcNow)
        {
            return _session
                .CreateCommand(_findReadyToExecuteJobs)
                .With("time", utcNow)
                .ExecuteToEnvelopes(_cancellation, _session.Transaction);
        }

        public void Dispose()
        {
            Session?.Dispose();
        }
    }
}
