using System;
using System.Linq;
using Baseline;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.LightningDb;
using Jasper.Testing.Bus;
using Shouldly;
using Xunit;

namespace IntegrationTests.LightningDb
{
    public class LocalPersistenceTester : IDisposable
    {
        private static Random random = new Random();
        private LocalPersistence thePersistence;

        public LocalPersistenceTester()
        {
            thePersistence = new LocalPersistence(new LightningDbSettings
            {
                QueuePath = AppContext.BaseDirectory.AppendPath("lmdbtest")
            });
            thePersistence.OpenDatabases(new string[]{"one", "two"});
            thePersistence.ClearAllStorage();

        }


        public void Dispose()
        {
            thePersistence?.Dispose();
        }

        private Envelope envelope()
        {
            var envelope = new Envelope
            {
                SentAt = DateTime.Today.ToUniversalTime(),
                Id = MessageId.GenerateRandom(),
                Queue = "incoming",
                SubQueue = "subqueue",
                Destination = "lq.tcp://localhost:2222/outgoing".ToUri(),
                MaxAttempts = 3,
                SentAttempts = 2,
                DeliverBy = DateTime.Today.ToUniversalTime()
            };

            envelope.Data = new byte[random.Next(100)];
            random.NextBytes(envelope.Data);

            envelope.Headers.Add("name", "Jeremy");
            envelope.Headers.Add("state", "Texas");
            envelope.Headers.Add("reply-uri", "lq.tcp://localhost:2221/replies");

            return envelope;
        }

        [Fact]
        public void save_and_load_an_envelope()
        {
            var original = envelope();
            thePersistence.Store("one", original);


            var loaded = thePersistence.Load("one", original.Id);

            loaded.Id.ShouldBe(original.Id);
            loaded.Data.ShouldHaveTheSameElementsAs(original.Data);
        }

        [Fact]
        public void save_and_load_several_envelopes()
        {
            var originals = new Envelope[50];
            for (int i = 0; i < originals.Length; i++)
            {
                originals[i] = envelope();
            }

            thePersistence.Store("one", originals);

            var loaded = thePersistence.LoadAll("one");

            loaded.Count.ShouldBe(originals.Length);
            foreach (var original in originals)
            {
                loaded.Any(x => Equals(x.Id, original.Id)).ShouldBeTrue();
            }


        }

        [Fact]
        public void save_to_multiple_databases()
        {
            var original1 = envelope();
            var original2 = envelope();

            thePersistence.Store("one", original1);
            thePersistence.Store("two", original2);

            thePersistence.LoadAll("one").Single().Id.ShouldBe(original1.Id);
            thePersistence.LoadAll("two").Single().Id.ShouldBe(original2.Id);
        }

        [Fact]
        public void delete_a_message()
        {
            var original1 = envelope();
            var original2 = envelope();

            thePersistence.Store("one", original1);
            thePersistence.Store("one", original2);

            thePersistence.Remove("one", original1);

            thePersistence.LoadAll("one").Single()
                .Id.ShouldBe(original2.Id);

        }

        [Fact]
        public void remove_several_envelopes()
        {
            var originals = new Envelope[50];
            for (int i = 0; i < originals.Length; i++)
            {
                originals[i] = envelope();
            }

            thePersistence.Store("one", originals);


            thePersistence.Remove("one", originals.Take(15));

            thePersistence.LoadAll("one").Count.ShouldBe(35);
        }


        [Fact]
        public void move_an_envelope()
        {
            var original1 = envelope();
            thePersistence.Store("one", original1);

            thePersistence.Move("one", "two", original1);


            thePersistence.LoadAll("one").Any().ShouldBeFalse();
            thePersistence.LoadAll("two").Single().Id
                .ShouldBe(original1.Id);

        }


    }
}
