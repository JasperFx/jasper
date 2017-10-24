using System;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Core;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.LightningDb.Testing
{
    public class LightningDbPersistenceTester : IDisposable
    {
        private static Random random = new Random();
        private LightningDbPersistence thePersistence;

        public LightningDbPersistenceTester()
        {
            thePersistence = new LightningDbPersistence(new LightningDbSettings
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
                EnvelopeVersionId = PersistedMessageId.GenerateRandom(),
                Queue = "incoming",
                SubQueue = "subqueue",
                Destination = "durable://localhost:2222/outgoing".ToUri(),
                SentAttempts = 2,
                DeliverBy = DateTime.Today.ToUniversalTime(),
                ReplyUri = "durable://localhost:2221/replies".ToUri()
            };

            envelope.Data = new byte[random.Next(100)];
            random.NextBytes(envelope.Data);

            envelope.Headers.Add("name", "Jeremy");
            envelope.Headers.Add("state", "Texas");

            return envelope;
        }

        [Fact]
        public void save_and_load_an_envelope()
        {
            var original = envelope();
            thePersistence.Store("one", original);


            var loaded = thePersistence.Load("one", original.EnvelopeVersionId);

            loaded.EnvelopeVersionId.ShouldBe(original.EnvelopeVersionId);
            loaded.Data.ShouldBe(original.Data);
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
                loaded.Any(x => Equals(x.EnvelopeVersionId, original.EnvelopeVersionId)).ShouldBeTrue();
            }


        }

        [Fact]
        public void save_to_multiple_databases()
        {
            var original1 = envelope();
            var original2 = envelope();

            thePersistence.Store("one", original1);
            thePersistence.Store("two", original2);

            thePersistence.LoadAll("one").Single().EnvelopeVersionId.ShouldBe(original1.EnvelopeVersionId);
            thePersistence.LoadAll("two").Single().EnvelopeVersionId.ShouldBe(original2.EnvelopeVersionId);
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
                .EnvelopeVersionId.ShouldBe(original2.EnvelopeVersionId);

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
            thePersistence.LoadAll("two").Single().EnvelopeVersionId
                .ShouldBe(original1.EnvelopeVersionId);

        }

        [Fact]
        public void persist_outgoing_batch_sent_attempts()
        {
            var envelopes = new Envelope[]
            {
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
            };

            thePersistence.Store(LightningDbPersistence.Outgoing, envelopes);

            envelopes[1].SentAttempts = 3;
            envelopes[1].Attempts = 1;

            envelopes[4].SentAttempts = 3;
            envelopes[4].Attempts = 1;


            var batch = new OutgoingMessageBatch("loopback://one".ToUri(), envelopes );

            thePersistence.PersistBasedOnSentAttempts(batch, 3);

            var loaded = thePersistence.LoadAll(LightningDbPersistence.Outgoing);

            loaded.Count.ShouldBe(5);

            loaded.Any(x => x.CorrelationId == envelopes[0].CorrelationId).ShouldBeTrue();
            loaded.Any(x => x.CorrelationId == envelopes[1].CorrelationId).ShouldBeFalse();
            loaded.Any(x => x.CorrelationId == envelopes[2].CorrelationId).ShouldBeTrue();
            loaded.Any(x => x.CorrelationId == envelopes[3].CorrelationId).ShouldBeTrue();
            loaded.Any(x => x.CorrelationId == envelopes[4].CorrelationId).ShouldBeFalse();
            loaded.Any(x => x.CorrelationId == envelopes[5].CorrelationId).ShouldBeTrue();
            loaded.Any(x => x.CorrelationId == envelopes[5].CorrelationId).ShouldBeTrue();
        }

        [Fact]
        public void can_replace_an_envelope()
        {
            var original = envelope();
            var id = original.EnvelopeVersionId;

            thePersistence.Store("one", original);

            original.SentAttempts = 2;

            thePersistence.Replace("one", original);

            original.EnvelopeVersionId.ShouldNotBe(id);

            thePersistence.Load("one", original.EnvelopeVersionId)
                .SentAttempts.ShouldBe(2);
        }

        [Fact]
        public void store_initial()
        {
            var envelopes = new Envelope[]
            {
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
            };

            foreach (var envelope in envelopes)
            {
                envelope.Queue = "two";
            }

            envelopes[1].Queue = "one";
            envelopes[4].Queue = "one";


            thePersistence.StoreInitial(envelopes);

            thePersistence.LoadAll("one").Count.ShouldBe(2);
            thePersistence.LoadAll("two").Count.ShouldBe(5);
        }

        [Fact]
        public void delete_a_mixed_batch_of_envelopes_by_database()
        {
            var envelopes = new Envelope[]
            {
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
                envelope(),
            };

            foreach (var envelope in envelopes)
            {
                envelope.Queue = "two";
            }

            envelopes[1].Queue = "one";
            envelopes[4].Queue = "one";


            thePersistence.StoreInitial(envelopes);

            thePersistence.Remove(new Envelope[]
            {
                envelopes[0],
                envelopes[1],
                envelopes[2],
            });

            thePersistence.LoadAll("one").Count.ShouldBe(1);
            thePersistence.LoadAll("two").Count.ShouldBe(3);

        }
    }
}
