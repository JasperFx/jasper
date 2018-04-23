using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Persistence;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests.Persistence
{
    public class SqlServerEnveloperPersistorTests : ConnectedContext
    {
        private readonly SqlServerEnvelopePersistor thePersistor
            = new SqlServerEnvelopePersistor(new SqlServerSettings
            {
                ConnectionString = ConnectionSource.ConnectionString
            });


        /*
         * IncrementIncomingEnvelopeAttempts
         * DiscardAndReassignOutgoing
         */

        [Fact]
        public async Task increment_the_attempt_count_of_incoming_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = TransportConstants.Incoming;

            await thePersistor.StoreIncoming(envelope);

            var prop = ReflectionHelper.GetProperty<Envelope>(x => x.Attempts);
            prop.SetValue(envelope, 3);

            await thePersistor.IncrementIncomingEnvelopeAttempts(envelope);

            var stored = thePersistor.AllIncomingEnvelopes().Single();
            stored.Attempts.ShouldBe(3);
        }

        [Fact]
        public async Task store_a_single_incoming_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = TransportConstants.Incoming;

            await thePersistor.StoreIncoming(envelope);

            var stored = thePersistor.AllIncomingEnvelopes().Single();

            stored.Id.ShouldBe(envelope.Id);
            stored.OwnerId.ShouldBe(envelope.OwnerId);
            stored.Status.ShouldBe(envelope.Status);
        }

        [Fact]
        public async Task store_multiple_incoming_envelopes()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());

            var stored = thePersistor.AllIncomingEnvelopes();

            list.Select(x => x.Id).OrderBy(x => x)
                .ShouldHaveTheSameElementsAs(stored.Select(x => x.Id).OrderBy(x => x));
        }

        [Fact]
        public async Task store_a_single_outgoing_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = TransportConstants.Outgoing;

            await thePersistor.StoreOutgoing(envelope, 5890);

            var stored = thePersistor.AllOutgoingEnvelopes().Single();

            stored.Id.ShouldBe(envelope.Id);
            stored.OwnerId.ShouldBe(5890);
            stored.Status.ShouldBe(envelope.Status);
        }

        [Fact]
        public async Task store_multiple_outgoing_envelopes()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 111);

            var stored = thePersistor.AllOutgoingEnvelopes();

            list.Select(x => x.Id).OrderBy(x => x)
                .ShouldHaveTheSameElementsAs(stored.Select(x => x.Id).OrderBy(x => x));

            stored.Each(x => x.OwnerId.ShouldBe(111));
        }

        [Fact]
        public async Task delete_a_single_outgoing_envelope()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 111);

            var toDelete = list[5];

            await thePersistor.DeleteOutgoingEnvelope(toDelete);

            var stored = thePersistor.AllOutgoingEnvelopes();
            stored.Count.ShouldBe(9);

            stored.Any(x => x.Id == toDelete.Id).ShouldBeFalse();

        }

        [Fact]
        public async Task delete_multiple_outgoing_envelope()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 111);

            var toDelete = new Envelope[] {list[2], list[3], list[7]};

            await thePersistor.DeleteOutgoingEnvelopes(toDelete);

            var stored = thePersistor.AllOutgoingEnvelopes();
            stored.Count.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[7].Id).ShouldBeFalse();

        }

        [Fact]
        public async Task delete_multiple_incoming_envelope()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());

            var toDelete = new Envelope[] {list[2], list[3], list[7]};

            await thePersistor.DeleteIncomingEnvelopes(toDelete);

            var stored = thePersistor.AllIncomingEnvelopes();
            stored.Count.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[7].Id).ShouldBeFalse();

        }

        [Fact]
        public async Task move_to_dead_letter_storage()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());


            var ex = new DivideByZeroException("Kaboom!");

            var report2 = new ErrorReport(list[2], ex);
            var report3 = new ErrorReport(list[3], ex);
            var report4 = new ErrorReport(list[4], ex);

            await thePersistor.MoveToDeadLetterStorage(new ErrorReport[] {report2, report3, report4});

            var stored = thePersistor.AllIncomingEnvelopes();

            stored.Count.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[4].Id).ShouldBeFalse();




        }

        [Fact]
        public async Task load_dead_letter_envelope()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());


            var ex = new DivideByZeroException("Kaboom!");

            var report2 = new ErrorReport(list[2], ex);
            var report3 = new ErrorReport(list[3], ex);
            var report4 = new ErrorReport(list[4], ex);

            await thePersistor.MoveToDeadLetterStorage(new ErrorReport[] {report2, report3, report4});


            var stored = await thePersistor.LoadDeadLetterEnvelope(report2.Id);

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
        public async Task schedule_execution()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());


            list[5].ExecutionTime = DateTimeOffset.Now.AddMinutes(5);

            list[7].ExecutionTime = DateTimeOffset.Now.AddMinutes(5);
            list[9].ExecutionTime = DateTimeOffset.Now.AddMinutes(5);

            await thePersistor.ScheduleExecution(new Envelope[] {list[5], list[7], list[9]});

            var stored = thePersistor.AllIncomingEnvelopes();
            stored.Count(x => x.Status == TransportConstants.Incoming).ShouldBe(7);
            stored.Count(x => x.Status == TransportConstants.Scheduled).ShouldBe(3);

            stored.Single(x => x.Id == list[5].Id).ExecutionTime.HasValue.ShouldBeTrue();
            stored.Single(x => x.Id == list[7].Id).ExecutionTime.HasValue.ShouldBeTrue();
            stored.Single(x => x.Id == list[9].Id).ExecutionTime.HasValue.ShouldBeTrue();
        }
    }
}
