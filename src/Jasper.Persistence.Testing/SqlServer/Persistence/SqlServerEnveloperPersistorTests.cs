using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using IntegrationTests;
using Jasper.Configuration;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.SqlServer.Persistence
{
    public class SqlServerEnveloperPersistorTests : SqlServerBackedListenerContext
    {
        public SqlServerEnveloperPersistorTests()
        {
            ThePersistence
                = new SqlServerEnvelopePersistence(new SqlServerSettings
                {
                    ConnectionString = Servers.SqlServerConnectionString
                }, new AdvancedSettings(null));

            ThePersistence.Admin.ClearAllPersistedEnvelopes().GetAwaiter().GetResult();
        }


        [Fact]
        public async Task delete_a_single_outgoing_envelope()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Outgoing;

                list.Add(envelope);
            }

            await ThePersistence.StoreOutgoing(list.ToArray(), 111);

            var toDelete = list[5];

            await ThePersistence.DeleteOutgoing(toDelete);

            var stored = ThePersistence.AllOutgoingEnvelopes();
            stored.Length.ShouldBe(9);

            stored.Any(x => x.Id == toDelete.Id).ShouldBeFalse();
        }

        [Fact]
        public async Task delete_multiple_incoming_envelope()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await ThePersistence.StoreIncoming(list.ToArray());

            var toDelete = new[] {list[2], list[3], list[7]};

            await ThePersistence.DeleteIncomingEnvelopes(toDelete);

            var stored = ThePersistence.AllIncomingEnvelopes();
            stored.Length.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[7].Id).ShouldBeFalse();
        }

        [Fact]
        public async Task delete_multiple_outgoing_envelope()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Outgoing;

                list.Add(envelope);
            }

            await ThePersistence.StoreOutgoing(list.ToArray(), 111);

            var toDelete = new[] {list[2], list[3], list[7]};

            await ThePersistence.DeleteOutgoing(toDelete);

            var stored = ThePersistence.AllOutgoingEnvelopes();
            stored.Length.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[7].Id).ShouldBeFalse();
        }

        [Fact]
        public async Task discard_and_reassign_outgoing()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Outgoing;

                list.Add(envelope);
            }

            await ThePersistence.StoreOutgoing(list.ToArray(), 111);

            var toDiscard = new[] {list[2], list[3], list[7]};
            var toReassign = new[] {list[1], list[4], list[6]};

            await ThePersistence.DiscardAndReassignOutgoing(toDiscard, toReassign, 444);

            var stored = ThePersistence.AllOutgoingEnvelopes();
            stored.Length.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[7].Id).ShouldBeFalse();

            stored.Single(x => x.Id == list[1].Id).OwnerId.ShouldBe(444);
            stored.Single(x => x.Id == list[4].Id).OwnerId.ShouldBe(444);
            stored.Single(x => x.Id == list[6].Id).OwnerId.ShouldBe(444);
        }

        [Fact]
        public async Task get_counts()
        {
            var list = new List<Envelope>();

            // 10 incoming
            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await ThePersistence.StoreIncoming(list.ToArray());


            // 7 scheduled
            list.Clear();
            for (var i = 0; i < 7; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Scheduled;

                list.Add(envelope);
            }

            await ThePersistence.StoreIncoming(list.ToArray());


            // 3 outgoing
            list.Clear();
            for (var i = 0; i < 3; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Outgoing;

                list.Add(envelope);
            }

            await ThePersistence.StoreOutgoing(list.ToArray(), 0);

            var counts = await ThePersistence.Admin.GetPersistedCounts();

            counts.Incoming.ShouldBe(10);
            counts.Scheduled.ShouldBe(7);
            counts.Outgoing.ShouldBe(3);
        }


        [Fact]
        public async Task increment_the_attempt_count_of_incoming_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = EnvelopeStatus.Incoming;

            await ThePersistence.StoreIncoming(envelope);

            var prop = ReflectionHelper.GetProperty<Envelope>(x => x.Attempts);
            prop.SetValue(envelope, 3);

            await ThePersistence.IncrementIncomingEnvelopeAttempts(envelope);

            var stored = ThePersistence.AllIncomingEnvelopes().Single();
            stored.Attempts.ShouldBe(3);
        }

        [Fact]
        public async Task load_dead_letter_envelope()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await ThePersistence.StoreIncoming(list.ToArray());


            var ex = new DivideByZeroException("Kaboom!");

            var report2 = new ErrorReport(list[2], ex);
            var report3 = new ErrorReport(list[3], ex);
            var report4 = new ErrorReport(list[4], ex);

            await ThePersistence.MoveToDeadLetterStorage(new[] {report2, report3, report4});


            var stored = await ThePersistence.Admin.LoadDeadLetterEnvelope(report2.Id);

            stored.ShouldNotBeNull();

            stored.ExceptionMessage.ShouldBe(report2.ExceptionMessage);
            stored.Id.ShouldBe(report2.Id);
            stored.ExceptionText.ShouldBe(report2.ExceptionText);
            stored.ExceptionType.ShouldBe(report2.ExceptionType);
            stored.Explanation.ShouldBe(report2.Explanation);
            stored.MessageType.ShouldBe(report2.MessageType);
            stored.RawData.ShouldBe(report2.RawData);
            stored.Source.ShouldBe(report2.Source);
        }

        [Fact]
        public async Task move_to_dead_letter_storage()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await ThePersistence.StoreIncoming(list.ToArray());


            var ex = new DivideByZeroException("Kaboom!");

            var report2 = new ErrorReport(list[2], ex);
            var report3 = new ErrorReport(list[3], ex);
            var report4 = new ErrorReport(list[4], ex);

            await ThePersistence.MoveToDeadLetterStorage(new[] {report2, report3, report4});

            var stored = ThePersistence.AllIncomingEnvelopes();

            stored.Length.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[4].Id).ShouldBeFalse();
        }

        [Fact]
        public async Task schedule_execution()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await ThePersistence.StoreIncoming(list.ToArray());


            list[5].ExecutionTime = DateTimeOffset.Now.AddMinutes(5);

            list[7].ExecutionTime = DateTimeOffset.Now.AddMinutes(5);
            list[9].ExecutionTime = DateTimeOffset.Now.AddMinutes(5);

            await ThePersistence.ScheduleExecution(new[] {list[5], list[7], list[9]});

            var stored = ThePersistence.AllIncomingEnvelopes();
            stored.Count(x => x.Status == EnvelopeStatus.Incoming).ShouldBe(7);
            stored.Count(x => x.Status == EnvelopeStatus.Scheduled).ShouldBe(3);

            stored.Single(x => x.Id == list[5].Id).ExecutionTime.HasValue.ShouldBeTrue();
            stored.Single(x => x.Id == list[7].Id).ExecutionTime.HasValue.ShouldBeTrue();
            stored.Single(x => x.Id == list[9].Id).ExecutionTime.HasValue.ShouldBeTrue();
        }

        [Fact]
        public async Task store_a_single_incoming_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = EnvelopeStatus.Incoming;

            await ThePersistence.StoreIncoming(envelope);

            var stored = ThePersistence.AllIncomingEnvelopes().Single();

            stored.Id.ShouldBe(envelope.Id);
            stored.OwnerId.ShouldBe(envelope.OwnerId);
            stored.Status.ShouldBe(envelope.Status);
        }

        [Fact]
        public async Task store_a_single_outgoing_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = EnvelopeStatus.Outgoing;

            await ThePersistence.StoreOutgoing(envelope, 5890);

            var stored = ThePersistence.AllOutgoingEnvelopes().Single();

            stored.Id.ShouldBe(envelope.Id);
            stored.OwnerId.ShouldBe(5890);
            stored.Status.ShouldBe(envelope.Status);
        }

        [Fact]
        public async Task store_multiple_incoming_envelopes()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Incoming;

                list.Add(envelope);
            }

            await ThePersistence.StoreIncoming(list.ToArray());

            var stored = ThePersistence.AllIncomingEnvelopes();

            list.Select(x => x.Id).OrderBy(x => x)
                .ShouldHaveTheSameElementsAs(stored.Select(x => x.Id).OrderBy(x => x));
        }

        [Fact]
        public async Task store_multiple_outgoing_envelopes()
        {
            var list = new List<Envelope>();

            for (var i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = EnvelopeStatus.Outgoing;

                list.Add(envelope);
            }

            await ThePersistence.StoreOutgoing(list.ToArray(), 111);

            var stored = ThePersistence.AllOutgoingEnvelopes();

            list.Select(x => x.Id).OrderBy(x => x)
                .ShouldHaveTheSameElementsAs(stored.Select(x => x.Id).OrderBy(x => x));

            stored.Each(x => x.OwnerId.ShouldBe(111));
        }
    }
}
