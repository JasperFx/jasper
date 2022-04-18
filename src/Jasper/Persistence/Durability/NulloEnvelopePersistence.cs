using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime.Scheduled;

namespace Jasper.Persistence.Durability
{
    public class NulloEnvelopePersistence : IEnvelopePersistence, IEnvelopeStorageAdmin
    {
        public IEnvelopeStorageAdmin Admin => this;
        public IScheduledJobProcessor ScheduledJobs { get; set; }

        public Task DeleteIncomingEnvelopesAsync(Envelope?[] envelopes)
        {
            return Task.CompletedTask;
        }

        public Task DeleteIncomingEnvelopeAsync(Envelope? envelope)
        {
            return Task.CompletedTask;
        }

        public Task DeleteOutgoingAsync(Envelope?[] envelopes)
        {
            return Task.CompletedTask;
        }

        public Task DeleteOutgoingAsync(Envelope? envelope)
        {
            return Task.CompletedTask;
        }

        public Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes)
        {
            throw new NotSupportedException();
        }

        public Task MoveToDeadLetterStorageAsync(ErrorReport[] errors)
        {
            return Task.CompletedTask;
        }

        public Task MoveToDeadLetterStorageAsync(Envelope? envelope, Exception? ex)
        {
            return Task.CompletedTask;
        }

        public Task ScheduleExecutionAsync(Envelope?[] envelopes)
        {
            return Task.CompletedTask;
        }

        public Task<ErrorReport> LoadDeadLetterEnvelopeAsync(Guid id)
        {
            return Task.FromResult<ErrorReport>(null);
        }

        public Task<IReadOnlyList<Envelope>> AllIncomingEnvelopes()
        {
            return Task.FromResult((IReadOnlyList<Envelope>)new List<Envelope>());
        }

        public Task<IReadOnlyList<Envelope>> AllOutgoingEnvelopes()
        {
            return Task.FromResult((IReadOnlyList<Envelope>)new List<Envelope>());
        }

        public Task ReleaseAllOwnership()
        {
            return Task.CompletedTask;
        }

        public Task CheckAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task IncrementIncomingEnvelopeAttemptsAsync(Envelope? envelope)
        {
            return Task.CompletedTask;
        }

        public Task StoreIncomingAsync(Envelope? envelope)
        {
            if (envelope.Status == EnvelopeStatus.Scheduled)
            {
                if (envelope.ScheduledTime == null) throw new ArgumentOutOfRangeException($"The envelope {envelope} is marked as Scheduled, but does not have an ExecutionTime");
                ScheduledJobs?.Enqueue(envelope.ScheduledTime.Value, envelope);
            }
            return Task.CompletedTask;
        }

        public Task StoreIncomingAsync(Envelope?[] envelopes)
        {
            foreach (var envelope in envelopes.Where(x => x.Status == EnvelopeStatus.Scheduled))
            {
                ScheduledJobs?.Enqueue(envelope.ScheduledTime.Value, envelope);
            }

            return Task.CompletedTask;
        }

        public Task<Uri?[]> FindAllDestinationsAsync()
        {
            throw new NotSupportedException();
        }

        public Task DiscardAndReassignOutgoingAsync(Envelope?[] discards, Envelope?[] reassigned, int nodeId)
        {
            return Task.CompletedTask;
        }

        public Task StoreOutgoingAsync(Envelope? envelope, int ownerId)
        {
            return Task.CompletedTask;
        }

        public Task StoreOutgoingAsync(Envelope[] envelopes, int ownerId)
        {
            return Task.CompletedTask;
        }

        public Task<PersistedCounts> GetPersistedCounts()
        {
            // Nothing to do, but keeps the metrics from blowing up
            return Task.FromResult(new PersistedCounts());
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("No persistent envelope storage");
        }

        public Task ClearAllPersistedEnvelopes()
        {
            return Task.CompletedTask;
        }

        public Task RebuildStorage()
        {
            return Task.CompletedTask;
        }

        public Task ScheduleJobAsync(Envelope? envelope)
        {
            ScheduledJobs?.Enqueue(envelope.ScheduledTime.Value, envelope);

            return Task.CompletedTask;
        }


        public void Dispose()
        {
            // Nothing
        }

        public IDurableStorageSession Session { get; } = null;
        public Task<IReadOnlyList<Envelope?>> LoadScheduledToExecuteAsync(DateTimeOffset utcNow)
        {
            throw new NotSupportedException();
        }

        public Task ReassignDormantNodeToAnyNodeAsync(int nodeId)
        {
            throw new NotSupportedException();
        }

        public Task<int[]> FindUniqueOwnersAsync(int currentNodeId)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Envelope?>> LoadOutgoingAsync(Uri? destination)
        {
            throw new NotSupportedException();
        }

        public Task ReassignOutgoingAsync(int ownerId, Envelope?[] outgoing)
        {
            throw new NotSupportedException();
        }

        public Task DeleteByDestinationAsync(Uri? destination)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Envelope?>> LoadPageOfGloballyOwnedIncomingAsync()
        {
            throw new NotSupportedException();
        }

        public Task ReassignIncomingAsync(int ownerId, IReadOnlyList<Envelope?> incoming)
        {
            throw new NotSupportedException();
        }
    }
}
