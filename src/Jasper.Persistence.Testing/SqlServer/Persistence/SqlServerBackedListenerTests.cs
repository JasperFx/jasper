using System.Linq;
using System.Threading.Tasks;
using Jasper.Transports;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    public class SqlServerBackedListenerTests : SqlServerBackedListenerContext
    {
        public SqlServerBackedListenerTests()
        {
            ThePersistence.Admin.ClearAllPersistedEnvelopes().GetAwaiter().GetResult();
        }

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
}
