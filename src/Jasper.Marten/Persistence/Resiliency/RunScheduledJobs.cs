using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.Operations;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Persistence;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Marten;
using Marten.Linq;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RunScheduledJobs : IMessagingAction
    {
        private readonly string _findReadyToExecuteJobs;
        private readonly IWorkerQueue _workers;
        private readonly IDocumentStore _store;
        private readonly EnvelopeTables _marker;
        private readonly ITransportLogger _logger;
        private readonly IRetries _retries;
        public static readonly int ScheduledJobLockId = "scheduled-jobs".GetHashCode();
        private readonly string _markOwnedIncomingSql;

        public RunScheduledJobs(IWorkerQueue workers, IDocumentStore store, EnvelopeTables marker, ITransportLogger logger, IRetries retries)
        {
            _workers = workers;
            _store = store;
            _marker = marker;
            _logger = logger;
            _retries = retries;

            _findReadyToExecuteJobs = $"select body from {marker.Incoming} where status = '{TransportConstants.Scheduled}' and execution_time <= :time";
            _markOwnedIncomingSql = $"update {marker.Incoming} set owner_id = :owner, status = '{TransportConstants.Incoming}' where id = ANY(:idlist)";

        }

        public async Task Execute(IDocumentSession session)
        {
            var utcNow = DateTimeOffset.UtcNow;;

            await ExecuteAtTime(session, utcNow);
        }

        public async Task<List<Envelope>> ExecuteAtTime(IDocumentSession session, DateTimeOffset utcNow)
        {
            if (!await session.TryGetGlobalTxLock(ScheduledJobLockId))
            {
                return null;
            }

            var readyToExecute = await session.Connection
                .CreateCommand(_findReadyToExecuteJobs)
                .With("time", utcNow, NpgsqlDbType.TimestampTZ)
                .ExecuteToEnvelopes();

            if (!readyToExecute.Any()) return readyToExecute;


            var identities = readyToExecute.Select(x => x.Id).ToArray();

            await session.Connection.CreateCommand()
                .Sql(_markOwnedIncomingSql)
                .With("idlist", identities, NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                .With("owner", _marker.CurrentNodeId, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync();

            await session.SaveChangesAsync();

            _logger.ScheduledJobsQueuedForExecution(readyToExecute);

            foreach (var envelope in readyToExecute)
            {
                envelope.Callback = new MartenCallback(envelope, _workers, _store, _marker, _retries);

                await _workers.Enqueue(envelope);
            }

            return readyToExecute;
        }
    }


}
