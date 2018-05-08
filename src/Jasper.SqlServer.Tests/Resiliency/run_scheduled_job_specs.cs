using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Resiliency;
using Jasper.SqlServer.Tests.Persistence;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests.Resiliency
{
    public class run_scheduled_job_specs : SqlServerBackedListenerContext
    {
        public run_scheduled_job_specs()
        {
            var logger = TransportLogger.Empty();

            theScheduledJob = new RunScheduledJobs(theWorkerQueue, mssqlSettings, logger, retries, theSettings);
        }

        private readonly RunScheduledJobs theScheduledJob;

        protected async Task<IReadOnlyList<Envelope>> afterExecutingAt(DateTimeOffset time)
        {
            var connection = new SqlConnection(ConnectionSource.ConnectionString);
            await connection.OpenAsync();

            try
            {
                return await theScheduledJob.ExecuteAtTime(connection, time);

            }
            finally
            {
                connection.Dispose();
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

            var persisted = thePersistor.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == env2.Id);

            persisted.Status.ShouldBe(TransportConstants.Incoming);
            persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);
        }
    }
}
