using System;
using System.Threading;
using System.Threading.Tasks;
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

        protected DurabilityAgentStorage(DatabaseSettings settings, JasperOptions options)
        {
            var transaction = new DurableStorageSession(settings, options.Cancellation);

            _session = transaction;
            Session = transaction;

            Nodes = new DurableNodes(transaction, settings, options.Cancellation);

            // ReSharper disable once VirtualMemberCallInConstructor
            Incoming = buildDurableIncoming(transaction, settings, options);

            // ReSharper disable once VirtualMemberCallInConstructor
            Outgoing = buildDurableOutgoing(transaction, settings, options);

            _findReadyToExecuteJobs =
                $"select body from {settings.SchemaName}.{IncomingTable} where status = '{TransportConstants.Scheduled}' and execution_time <= @time";

            _cancellation = options.Cancellation;
        }

        protected abstract IDurableOutgoing buildDurableOutgoing(DurableStorageSession durableStorageSession, DatabaseSettings settings, JasperOptions options);

        protected abstract IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession, DatabaseSettings settings, JasperOptions options);

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