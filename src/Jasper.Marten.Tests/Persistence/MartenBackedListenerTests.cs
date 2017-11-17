using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.WorkerQueues;
using Jasper.Marten.Persistence;
using Jasper.Marten.Tests.Setup;
using Jasper.Util;
using Marten;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence
{


    public class MartenBackedListenerTests : IDisposable
    {
        protected readonly Uri theUri = "tcp://localhost:1111".ToUri();
        protected readonly DocumentStore theStore;
        protected IWorkerQueue theWorkerQueue;
        protected BusSettings theSettings;
        protected MartenBackedListener theListener;

        protected readonly IList<Envelope> theEnvelopes = new List<Envelope>();


        public MartenBackedListenerTests()
        {
            theStore = DocumentStore.For(ConnectionSource.ConnectionString);

            theStore.Advanced.Clean.CompletelyRemoveAll();

            theWorkerQueue = Substitute.For<IWorkerQueue>();

            theSettings = new BusSettings();

            theListener = new MartenBackedListener(
                Substitute.For<IListeningAgent>(),
                theWorkerQueue,
                theStore,
                CompositeLogger.Empty(), theSettings);
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
                return await session.Query<Envelope>().ToListAsync();
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


        [Fact]
        public async Task handling_a_single_not_scheduled_envelope()
        {
            var envelope = notScheduledEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(TransportConstants.Incoming);
            persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);

            assertEnvelopeWasEnqueued(envelope);
        }

        [Fact]
        public async Task handling_a_single_scheduled_but_expired_envelope()
        {
            var envelope = scheduledButExpiredEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(TransportConstants.Incoming);
            persisted.OwnerId.ShouldBe(theSettings.UniqueNodeId);

            assertEnvelopeWasEnqueued(envelope);
        }

        [Fact]
        public async Task handling_a_single_scheduled_envelope()
        {
            var envelope = scheduledEnvelope();
            var persisted = (await afterReceivingTheEnvelopes()).Single();

            persisted.Status.ShouldBe(TransportConstants.Scheduled);
            persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);

            assertEnvelopeWasNotEnqueued(envelope);
        }
    }
}
