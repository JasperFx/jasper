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
        public readonly InMemoryEnvelopePersistor thePersistor = new InMemoryEnvelopePersistor();

        [Fact]
        public void persist_single()
        {
            var envelope = ObjectMother.Envelope();

            thePersistor.Persist(envelope);
            thePersistor.Persist(envelope);

            thePersistor.Queued.Single().ShouldBeSameAs(envelope);
        }

        [Fact]
        public void persist_multiple()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            thePersistor.Persist(new Envelope[]{envelope1, envelope2, envelope3});
            thePersistor.Persist(new Envelope[]{envelope1, envelope2, envelope3});

            thePersistor.Queued.ShouldHaveTheSameElementsAs(envelope1, envelope2, envelope3);
        }

        [Fact]
        public void persist_at_different_points()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            thePersistor.Persist(envelope1);
            thePersistor.Persist(envelope2);
            thePersistor.Persist(envelope3);

            thePersistor.Queued.ShouldHaveTheSameElementsAs(envelope1, envelope2, envelope3);
        }

        [Fact]
        public void schedule_job()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            thePersistor.ScheduleJob(envelope1);
            thePersistor.Persist(envelope2);
            thePersistor.ScheduleJob(envelope3);
            thePersistor.ScheduleJob(envelope3);

            thePersistor.Scheduled.ShouldHaveTheSameElementsAs(envelope1, envelope3);
        }

        [Fact]
        public async Task copy_to()
        {
            var envelope1 = ObjectMother.Envelope();
            var envelope2 = ObjectMother.Envelope();
            var envelope3 = ObjectMother.Envelope();

            await thePersistor.ScheduleJob(envelope1);
            await thePersistor.Persist(envelope2);
            await thePersistor.ScheduleJob(envelope3);
            await thePersistor.ScheduleJob(envelope3);

            var other = new InMemoryEnvelopePersistor();

            await thePersistor.CopyTo(other);

            thePersistor.Queued.ShouldHaveTheSameElementsAs(envelope2);
            thePersistor.Scheduled.ShouldHaveTheSameElementsAs(envelope1, envelope3);
        }
    }
}
