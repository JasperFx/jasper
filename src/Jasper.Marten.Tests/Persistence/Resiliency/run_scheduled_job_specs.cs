using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Marten;
using Marten.Services;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence.Resiliency
{
    public class run_scheduled_job_specs : MartenBackedListenerContext
    {
        private RunScheduledJobs theScheduledJob;

        public run_scheduled_job_specs()
        {
            var logger = TransportLogger.Empty();
            var envelopeTables = new EnvelopeTables(theSettings, new StoreOptions());
            var retries = new EnvelopeRetries(new MartenEnvelopePersistor(theStore, envelopeTables), logger, theSettings);

            theScheduledJob = new RunScheduledJobs(theWorkerQueue, theStore, envelopeTables, logger, retries);
        }

        protected async Task<IReadOnlyList<Envelope>> afterExecutingAt(DateTimeOffset time)
        {
            var connection = theStore.Tenancy.Default.CreateConnection();
            await connection.OpenAsync();
            var tx = connection.BeginTransaction();

            using (var session = theStore.OpenSession(new SessionOptions
            {
                Transaction = tx,
                Tracking = DocumentTracking.None
            }))
            {

                var envelopes = await theScheduledJob.ExecuteAtTime(session, time);

                await tx.CommitAsync();

                return envelopes;
            }


        }

        [Fact]
        public async Task run_when_there_are_no_expired_jobs()
        {
            var env1 = notScheduledEnvelope();
            var env2 = scheduledEnvelope();

            await afterReceivingTheEnvelopes();

            // Will not trigger anything here
            (await afterExecutingAt(DateTime.UtcNow)).Any().ShouldBeFalse();
        }

        [Fact]
        public async Task run_when_there_is_a_single_expired_job()
        {
            var env1 = notScheduledEnvelope();
            var env2 = scheduledEnvelope();

            await afterReceivingTheEnvelopes();

            var executed = await afterExecutingAt(env2.ExecutionTime.Value.AddHours(1));

            var executed1 = executed.Single();

            executed1.Id.ShouldBe(env2.Id);
            await theWorkerQueue.Received().Enqueue(executed1);

            using (var session = theStore.QuerySession())
            {
                var persisted = session.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == env2.Id);

                persisted.Status.ShouldBe(TransportConstants.Incoming);
                persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);
            }
        }


    }


}
