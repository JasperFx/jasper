using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Marten.Tests.Setup;
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
            theScheduledJob = new RunScheduledJobs(theWorkerQueue, theStore, new OwnershipMarker(theSettings, new StoreOptions()));
        }

        protected async Task<IReadOnlyList<Envelope>> afterExecutingAt(DateTime time)
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
            theWorkerQueue.Received().Enqueue(executed1);

            using (var session = theStore.QuerySession())
            {
                var persisted = await session.LoadAsync<Envelope>(env2.Id);
                persisted.Status.ShouldBe(TransportConstants.Incoming);
                persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);
            }
        }


    }


}
