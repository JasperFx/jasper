using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Persistence;
using Jasper.Messaging.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class InMemoryEnvelopePersistorTester
    {
        public readonly InMemoryEnvelopeTransaction theTransaction = new InMemoryEnvelopeTransaction();

        [Fact]
        public void persist_single()
        {
            var envelope = ObjectMother.Envelope();

            theTransaction.Persist(envelope);
            theTransaction.Persist(envelope);

            theTransaction.Queued.Single().ShouldBeSameAs(envelope);
        }

        [Fact]
        public void persist_multiple()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            theTransaction.Persist(new Envelope[]{envelope1, envelope2, envelope3});
            theTransaction.Persist(new Envelope[]{envelope1, envelope2, envelope3});

            theTransaction.Queued.ShouldHaveTheSameElementsAs(envelope1, envelope2, envelope3);
        }

        [Fact]
        public void persist_at_different_points()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            theTransaction.Persist(envelope1);
            theTransaction.Persist(envelope2);
            theTransaction.Persist(envelope3);

            theTransaction.Queued.ShouldHaveTheSameElementsAs(envelope1, envelope2, envelope3);
        }

        [Fact]
        public void schedule_job()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            theTransaction.ScheduleJob(envelope1);
            theTransaction.Persist(envelope2);
            theTransaction.ScheduleJob(envelope3);
            theTransaction.ScheduleJob(envelope3);

            theTransaction.Scheduled.ShouldHaveTheSameElementsAs(envelope1, envelope3);
        }

        [Fact]
        public async Task copy_to()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            await theTransaction.ScheduleJob(envelope1);
            await theTransaction.Persist(envelope2);
            await theTransaction.ScheduleJob(envelope3);
            await theTransaction.ScheduleJob(envelope3);

            var other = new InMemoryEnvelopeTransaction();

            await theTransaction.CopyTo(other);

            theTransaction.Queued.ShouldHaveTheSameElementsAs(envelope2);
            theTransaction.Scheduled.ShouldHaveTheSameElementsAs(envelope1, envelope3);
        }
    }
}
