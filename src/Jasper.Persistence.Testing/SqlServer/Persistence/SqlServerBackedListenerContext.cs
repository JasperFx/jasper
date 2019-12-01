using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.WorkerQueues;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Util;
using NSubstitute;
using Shouldly;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    public class SqlServerBackedListenerContext : SqlServerContext
    {
        protected readonly IList<Envelope> theEnvelopes = new List<Envelope>();
        protected readonly Uri theUri = "tcp://localhost:1111".ToUri();
        protected SqlServerSettings mssqlSettings;
        protected SqlServerEnvelopePersistence ThePersistence;
        protected AdvancedSettings theSettings;
        protected DurableWorkerQueue theWorkerQueue;
        private IHandlerPipeline thePipeline;


        public SqlServerBackedListenerContext()
        {
            new SqlServerEnvelopeStorageAdmin(new SqlServerSettings{ConnectionString = Servers.SqlServerConnectionString}).RecreateAll();


            theSettings = new AdvancedSettings();

            mssqlSettings = new SqlServerSettings
            {
                ConnectionString = Servers.SqlServerConnectionString
            };

            ThePersistence = new SqlServerEnvelopePersistence(mssqlSettings, theSettings);


            thePipeline = Substitute.For<IHandlerPipeline>();
            theWorkerQueue = new DurableWorkerQueue(new Endpoint(), thePipeline, theSettings, ThePersistence, TransportLogger.Empty());

        }

        protected Envelope notScheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4}
            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                ExecutionTime = DateTime.UtcNow.Add(1.Hours())
            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledButExpiredEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[] {1, 2, 3, 4},
                ExecutionTime = DateTime.UtcNow.Add(-1.Hours())
            };

            theEnvelopes.Add(env);

            return env;
        }

        protected async Task<IReadOnlyList<Envelope>> afterReceivingTheEnvelopes()
        {
            var status = await theWorkerQueue.ProcessReceivedMessages(DateTime.UtcNow, theUri, theEnvelopes.ToArray());

            status.ShouldBe(ReceivedStatus.Successful);

            return ThePersistence.AllIncomingEnvelopes();
        }

        protected void assertEnvelopeWasEnqueued(Envelope envelope)
        {
            thePipeline.Received().Invoke(envelope);
        }

        protected void assertEnvelopeWasNotEnqueued(Envelope envelope)
        {
            thePipeline.DidNotReceive().Invoke(envelope);
        }
    }
}
