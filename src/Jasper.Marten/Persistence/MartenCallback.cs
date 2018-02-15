using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Marten;
using Marten.Events;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence
{
    public class MartenCallback : IMessageCallback
    {
        private readonly Envelope _envelope;
        private readonly IWorkerQueue _queue;
        private readonly IDocumentStore _store;
        private readonly EnvelopeTables _marker;
        private readonly MartenRetries _retries;
        private readonly ITransportLogger _logger;

        public MartenCallback(Envelope envelope, IWorkerQueue queue, IDocumentStore store, EnvelopeTables marker, MartenRetries retries, ITransportLogger logger)
        {
            _envelope = envelope;
            _queue = queue;
            _store = store;
            _marker = marker;
            _retries = retries;
            _logger = logger;
        }

        public Task MarkComplete()
        {
            _retries.DeleteIncoming(_envelope);

            return Task.CompletedTask;
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            _retries.LogErrorReport(new ErrorReport(envelope, exception));
            return Task.CompletedTask;
        }

        public async Task Requeue(Envelope envelope)
        {
            envelope.Attempts++;

            try
            {
                using (var conn = _store.Tenancy.Default.CreateConnection())
                {
                    await conn.OpenAsync();

                    await conn.CreateCommand($"update {_marker.Incoming} set attempts = :attempts where id = :id")
                        .With("attempts", envelope.Attempts, NpgsqlDbType.Integer)
                        .With("id", envelope.Id, NpgsqlDbType.Uuid)
                        .ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                // Not going to worry about a failure here

            }

            await _queue.Enqueue(envelope);
        }

        public Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope)
        {
            envelope.ExecutionTime = time;
            envelope.Status = TransportConstants.Scheduled;
            _retries.ScheduleExecution(envelope);


            return Task.CompletedTask;
        }

        private async Task moveToScheduled(DateTimeOffset time, Envelope envelope)
        {
            envelope.Attempts++;
            envelope.ExecutionTime = time;
            envelope.Status = TransportConstants.Scheduled;

            _retries.ScheduleExecution(envelope);
        }
    }
}
