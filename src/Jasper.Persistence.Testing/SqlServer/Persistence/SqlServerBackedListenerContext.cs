using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Logging;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports.Local;
using Jasper.Transports.Tcp;
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


            theSettings = new AdvancedSettings(null);

            mssqlSettings = new SqlServerSettings
            {
                ConnectionString = Servers.SqlServerConnectionString
            };

            ThePersistence = new SqlServerEnvelopePersistence(mssqlSettings, theSettings);


            thePipeline = Substitute.For<IHandlerPipeline>();
            theWorkerQueue = new DurableWorkerQueue(new LocalQueueSettings("temp"), thePipeline, theSettings, ThePersistence, TransportLogger.Empty());

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
            await theWorkerQueue.ProcessReceivedMessages(DateTime.UtcNow, theUri, theEnvelopes.ToArray());

            return ThePersistence.AllIncomingEnvelopes();
        }

        protected void assertEnvelopeWasEnqueued(Envelope envelope)
        {
            thePipeline.Received().Invoke(envelope, theWorkerQueue);
        }

        protected void assertEnvelopeWasNotEnqueued(Envelope envelope)
        {
            thePipeline.DidNotReceive().Invoke(envelope, theWorkerQueue);
        }
    }
}
