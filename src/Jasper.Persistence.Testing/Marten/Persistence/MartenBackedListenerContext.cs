using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Postgresql.Schema;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Tcp;
using Jasper.Util;
using Marten;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class MartenBackedListenerTests : MartenBackedListenerContext
    {
        [Fact]
        public async Task handling_a_single_not_scheduled_envelope()
        {
            var envelope = notScheduledEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(EnvelopeStatus.Incoming);
            persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);
            persisted.ReceivedAt.ShouldBe(theUri);

            assertEnvelopeWasEnqueued(envelope);
        }

        [Fact]
        public async Task handling_a_single_scheduled_but_expired_envelope()
        {
            var envelope = scheduledButExpiredEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(EnvelopeStatus.Incoming);
            persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);
            persisted.ReceivedAt.ShouldBe(theUri);

            assertEnvelopeWasEnqueued(envelope);
        }

        [Fact]
        public async Task handling_a_single_scheduled_envelope()
        {
            var envelope = scheduledEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(EnvelopeStatus.Scheduled);
            persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
            persisted.ReceivedAt.ShouldBe(theUri);

            assertEnvelopeWasNotEnqueued(envelope);
        }
    }

    public class MartenBackedListenerContext : PostgresqlContext, IDisposable
    {
        protected readonly IEnvelopeStorageAdmin EnvelopeStorageAdmin =
            new PostgresqlEnvelopeStorageAdmin(new PostgresqlSettings
                {ConnectionString = Servers.PostgresConnectionString});

        protected readonly IList<Envelope> theEnvelopes = new List<Envelope>();
        protected readonly DocumentStore theStore;
        protected readonly Uri theUri = "tcp://localhost:1111".ToUri();
        private readonly IHandlerPipeline thePipeline;
        protected AdvancedSettings theSettings;
        protected DurableWorkerQueue theWorkerQueue;


        public MartenBackedListenerContext()
        {
            theStore = DocumentStore.For(_ =>
            {
                _.Connection(Servers.PostgresConnectionString);
                _.PLV8Enabled = false;
            });


            theSettings = new AdvancedSettings(null);


            EnvelopeStorageAdmin.RebuildSchemaObjects();

            var persistence =
                new PostgresqlEnvelopePersistence(
                    new PostgresqlSettings {ConnectionString = Servers.PostgresConnectionString}, theSettings);
            thePipeline = Substitute.For<IHandlerPipeline>();
            theWorkerQueue = new DurableWorkerQueue(new LocalQueueSettings("temp"), thePipeline, theSettings,
                persistence, TransportLogger.Empty());


            var agent = Substitute.For<IListener>();
            theWorkerQueue.StartListening(agent);
        }

        public void Dispose()
        {
            theStore?.Dispose();
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

        protected async Task<Envelope[]> afterReceivingTheEnvelopes()
        {
            await theWorkerQueue.ProcessReceivedMessages(DateTime.UtcNow, theUri, theEnvelopes.ToArray());

            return await EnvelopeStorageAdmin.AllIncomingEnvelopes();
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
