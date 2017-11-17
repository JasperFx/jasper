using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Marten;
using Marten.Linq;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RunScheduledJobs : IMessagingAction
    {

        private readonly IWorkerQueue _workers;
        private readonly IDocumentStore _store;
        private readonly BusSettings _settings;
        public static readonly int ScheduledJobLockId = "scheduled-jobs".GetHashCode();
        private readonly string _markIncomingSql;

        public RunScheduledJobs(IWorkerQueue workers, IDocumentStore store, BusSettings settings, StoreOptions storeConfiguration)
        {
            _workers = workers;
            _store = store;
            _settings = settings;

            var dbObjectName = storeConfiguration.Storage.MappingFor(typeof(Envelope)).Table;
            _markIncomingSql = $"update {dbObjectName} set data = data || '{{\"{nameof(Envelope.Status)}\": \"{TransportConstants.Incoming}\", \"{nameof(Envelope.OwnerId)}\": \"{settings.UniqueNodeId}\"}}' where id = ANY(:idlist)";
        }

        public async Task Execute(IDocumentSession session)
        {
            var utcNow = DateTime.UtcNow;

            await ExecuteAtTime(session, utcNow);
        }

        public async Task<Envelope[]> ExecuteAtTime(IDocumentSession session, DateTime utcNow)
        {
            if (!await session.TryGetGlobalTxLock(ScheduledJobLockId))
            {
                return null;
            }

            var readyToExecute = (await session.QueryAsync(new FindScheduledJobsReadyToGo(utcNow))).ToArray();

            var identities = readyToExecute.Select(x => x.Id).ToArray();

            if (!identities.Any())
            {
                return new Envelope[0];
            }


            await markAsIncomingOwnedByThisNode(session, identities);


            foreach (var envelope in readyToExecute)
            {
                envelope.Callback = new MartenCallback(envelope, _workers, _store);

                await _workers.Enqueue(envelope);
            }

            return readyToExecute;
        }

        private async Task markAsIncomingOwnedByThisNode(IDocumentSession session, string[] identities)
        {
            await session.Connection.CreateCommand()
                .Sql(_markIncomingSql)
                .With("idlist", identities, NpgsqlDbType.Array | NpgsqlDbType.Varchar)
                .ExecuteNonQueryAsync();

            await session.SaveChangesAsync();
        }
    }

    public class FindScheduledJobsReadyToGo : ICompiledListQuery<Envelope>
    {
        public DateTime CurrentTime { get; set; }
        public string Status { get; set; } = TransportConstants.Scheduled;

        public FindScheduledJobsReadyToGo()
        {
        }

        public FindScheduledJobsReadyToGo(DateTime currentTime)
        {
            CurrentTime = currentTime;
        }

        public Expression<Func<IQueryable<Envelope>, IEnumerable<Envelope>>> QueryIs()
        {
            return q => q
                .Where(x => x.Status == Status && x.ExecutionTime <= CurrentTime);
        }
    }
}
