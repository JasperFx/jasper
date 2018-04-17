using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.DbObjects;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Marten;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence
{

    public class MartenBackedListenerTests : MartenBackedListenerContext
    {
        [Fact]
        public async Task handling_a_single_not_scheduled_envelope()
        {
            var envelope = notScheduledEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(TransportConstants.Incoming);
            persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);
            persisted.ReceivedAt.ShouldBe(theUri);

            assertEnvelopeWasEnqueued(envelope);
        }

        [Fact]
        public async Task handling_a_single_scheduled_but_expired_envelope()
        {
            var envelope = scheduledButExpiredEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(TransportConstants.Incoming);
            persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);
            persisted.ReceivedAt.ShouldBe(theUri);

            assertEnvelopeWasEnqueued(envelope);
        }

        [Fact]
        public async Task handling_a_single_scheduled_envelope()
        {
            var envelope = scheduledEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(TransportConstants.Scheduled);
            persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
            persisted.ReceivedAt.ShouldBe(theUri);

            assertEnvelopeWasNotEnqueued(envelope);
        }
    }

    public class MartenBackedListenerContext : IDisposable
    {
        protected readonly Uri theUri = "tcp://localhost:1111".ToUri();
        protected readonly DocumentStore theStore;
        protected IWorkerQueue theWorkerQueue;
        protected MessagingSettings theSettings;
        protected MartenBackedListener theListener;

        protected readonly IList<Envelope> theEnvelopes = new List<Envelope>();


        public MartenBackedListenerContext()
        {
            theStore = DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.PLV8Enabled = false;
                _.Storage.Add<PostgresqlEnvelopeStorage>();
            });

            theStore.Advanced.Clean.CompletelyRemoveAll();

            theStore.Schema.ApplyAllConfiguredChangesToDatabase();

            theWorkerQueue = Substitute.For<IWorkerQueue>();

            theSettings = new MessagingSettings();

            var tables = new EnvelopeTables(theSettings, new StoreOptions());

            var retries = new MartenRetries(theStore, tables, TransportLogger.Empty(), theSettings);


            theListener = new MartenBackedListener(
                Substitute.For<IListeningAgent>(),
                theWorkerQueue,
                theStore,
                TransportLogger.Empty(), theSettings, tables, retries);
        }

        public void Dispose()
        {

            theStore?.Dispose();
        }

        protected Envelope notScheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[]{1,2,3,4}

            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[]{1,2,3,4},
                ExecutionTime = DateTime.UtcNow.Add(1.Hours())

            };

            theEnvelopes.Add(env);

            return env;
        }

        protected Envelope scheduledButExpiredEnvelope()
        {
            var env = new Envelope
            {
                Data = new byte[]{1,2,3,4},
                ExecutionTime = DateTime.UtcNow.Add(-1.Hours())

            };

            theEnvelopes.Add(env);

            return env;
        }

        protected async Task<IReadOnlyList<Envelope>> afterReceivingTheEnvelopes()
        {
            var status = await theListener.Received(theUri, theEnvelopes.ToArray());

            status.ShouldBe(ReceivedStatus.Successful);

            using (var session = theStore.QuerySession())
            {
                return session.AllIncomingEnvelopes();
            }
        }

        protected void assertEnvelopeWasEnqueued(Envelope envelope)
        {
            theWorkerQueue.Received().Enqueue(envelope);
        }

        protected void assertEnvelopeWasNotEnqueued(Envelope envelope)
        {
            theWorkerQueue.DidNotReceive().Enqueue(envelope);
        }



    }
}
