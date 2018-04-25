using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Persistence;
using Jasper.SqlServer.Util;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests.Persistence.Outbox
{
    public class outbox_usage : IDisposable
    {
        private JasperRuntime theSender;
        private JasperRuntime theReceiver;
        private MessageTracker theTracker;
        private SqlServerEnvelopePersistor thePersistor;

        public outbox_usage()
        {
            theSender = JasperRuntime.For<ItemSender>();
            theReceiver = JasperRuntime.For<ItemReceiver>();
            theTracker = theReceiver.Get<MessageTracker>();

            theSender.RebuildMessageStorage();
            theReceiver.RebuildMessageStorage();

            thePersistor = theReceiver.Get<SqlServerEnvelopePersistor>();

            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                conn.Open();

                conn.CreateCommand(@"
IF OBJECT_ID('receiver.item_created', 'U') IS NOT NULL
  drop table receiver.item_created;

").ExecuteNonQuery();

                conn.CreateCommand(@"
create table receiver.item_created
(
	id uniqueidentifier not null
		primary key,
	name varchar(100) not null
);

").ExecuteNonQuery();
            }

        }

        private async Task<ItemCreated> loadItem(Guid id)
        {
            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn.OpenAsync();

                var name = (string) await conn.CreateCommand("select name from receiver.item_created where id = @id")
                    .With("id", id)
                    .ExecuteScalarAsync();

                if (name.IsEmpty())
                {
                    return null;
                }

                return new ItemCreated
                {
                    Id = id,
                    Name = name
                };
            }
        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        [Fact]
        public async Task send_and_await_still_works()
        {
            var bus = theSender.Get<IMessageContext>();

            var item = new ItemCreated
            {
                Name = "Hat",
                Id = Guid.NewGuid()
            };

            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn.OpenAsync();

                var tx = conn.BeginTransaction();
                await bus.EnlistInTransaction(tx);

                await bus.SendAndWait(item);
            }




            var item2 = await loadItem(item.Id);

            item2.Name.ShouldBe("Hat");


            var deleted = thePersistor.AllIncomingEnvelopes().Any();
            if (!deleted)
            {
                Thread.Sleep(500);
                thePersistor.AllIncomingEnvelopes().Any().ShouldBeFalse();
            }
        }

        [Fact]
        public async Task request_reply_still_works()
        {
            var bus = theSender.Get<IMessageContext>();

            var question = new Question
            {
                X = 3,
                Y = 4
            };

            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn.OpenAsync();

                var tx = conn.BeginTransaction();
                await bus.EnlistInTransaction(tx);

                var answer = await bus.Request<Answer>(question);
                answer.Sum.ShouldBe(7);
            }
        }

        [Fact]
        public async Task basic_send()
        {
            var bus = theSender.Get<IMessageContext>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn.OpenAsync();

                var tx = conn.BeginTransaction();
                await bus.EnlistInTransaction(tx);

                await bus.Send(item);

                tx.Commit();

                await bus.SendAllQueuedOutgoingMessages();
            }


            waiter.Wait(5.Seconds());

            waiter.IsCompleted.ShouldBeTrue();

            var item2 = await loadItem(item.Id);
            if (item2 == null)
            {
                Thread.Sleep(500);
                item2 = await loadItem(item.Id);
            }



            item2.Name.ShouldBe("Shoe");

            if (!thePersistor.AllIncomingEnvelopes().Any())
            {
                Thread.Sleep(500);
                thePersistor.AllIncomingEnvelopes().Any().ShouldBeFalse();
            }

            var allGone = theSender.Get<SqlServerEnvelopePersistor>().AllOutgoingEnvelopes().Any();
            if (allGone)
            {
                Thread.Sleep(500);
                theSender.Get<SqlServerEnvelopePersistor>().AllOutgoingEnvelopes().Any().ShouldBeFalse();
            }
        }

        [Fact]
        public async Task send_with_receiver_down_to_see_the_persisted_envelopes()
        {
            // Shutting it down
            theReceiver.Dispose();
            theReceiver = null;

            var bus = theSender.Get<IMessageContext>();
            var senderPersistor = theSender.Get<SqlServerEnvelopePersistor>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn.OpenAsync();

                var tx = conn.BeginTransaction();

                await bus.EnlistInTransaction(tx);

                await bus.Send(item);

                tx.Commit();

                await bus.SendAllQueuedOutgoingMessages();
            }



            var outgoing = senderPersistor.AllOutgoingEnvelopes().SingleOrDefault();

            outgoing.MessageType.ShouldBe(typeof(ItemCreated).ToMessageAlias());
        }

        [Fact]
        public async Task schedule_local_job_durably()
        {
            var bus = theSender.Get<IMessageContext>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            using (var conn = new SqlConnection(ConnectionSource.ConnectionString))
            {
                await conn.OpenAsync();

                var tx = conn.BeginTransaction();


                await bus.EnlistInTransaction(tx);

                await bus.Schedule(item, 1.Hours());



                tx.Commit();

                await bus.SendAllQueuedOutgoingMessages();
            }

            var scheduled = theSender.Get<SqlServerEnvelopePersistor>().AllIncomingEnvelopes().Single();

            scheduled.MessageType.ShouldBe(typeof(ItemCreated).ToMessageAlias());
            scheduled.Status.ShouldBe(TransportConstants.Scheduled);
        }
    }
}
