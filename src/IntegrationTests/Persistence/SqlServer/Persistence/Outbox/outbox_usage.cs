using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alba;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Util;
using Jasper.TestSupport.Alba;
using Jasper.Util;
using Servers;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer.Persistence.Outbox
{
    public class outbox_usage : SqlServerContext, IDisposable
    {
        private JasperRuntime theSender;
        private JasperRuntime theReceiver;
        private MessageTracker theTracker;
        private SqlServerEnvelopePersistor thePersistor;

        public outbox_usage(DockerFixture<SqlServerContainer> fixture) : base(fixture)
        {
            theSender = JasperRuntime.For<ItemSender>();
            theReceiver = JasperRuntime.For<ItemReceiver>();
            theTracker = theReceiver.Get<MessageTracker>();

            theSender.RebuildMessageStorage();
            theReceiver.RebuildMessageStorage();

            thePersistor = theReceiver.Get<SqlServerEnvelopePersistor>();

            using (var conn = new SqlConnection(SqlServerContainer.ConnectionString))
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
            using (var conn = new SqlConnection(SqlServerContainer.ConnectionString))
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

            using (var conn = new SqlConnection(SqlServerContainer.ConnectionString))
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

            using (var conn = new SqlConnection(SqlServerContainer.ConnectionString))
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
            var messaging = theSender.Get<IMessageContext>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            await outbox_sample(messaging, item);


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
        public async Task usage_within_http_route()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            await theSender.Scenario(_ =>
            {
                _.Post.Json(item).ToUrl("/send/item");
                _.StatusCodeShouldBeOk();
            });


            waiter.Wait(5.Seconds());

            waiter.IsCompleted.ShouldBeTrue();
        }

        // SAMPLE: basic-sql-server-outbox-sample
        private static async Task outbox_sample(IMessageContext messaging, ItemCreated item)
        {
            using (var conn = new SqlConnection(SqlServerContainer.ConnectionString))
            {
                await conn.OpenAsync();

                // Start your transaction
                var tx = conn.BeginTransaction();

                // enlist the current IMessageContext to persist
                // outgoing messages with the current transaction
                await messaging.EnlistInTransaction(tx);

                // enqueue outgoing messages
                await messaging.Send(item);

                tx.Commit();

                // Flush the outgoing messages into Jasper's outgoing
                // sending loops, but don't worry, the messages are
                // persisted where they can be recovered by other nodes
                // in case something goes wrong here
                await messaging.SendAllQueuedOutgoingMessages();
            }
        }
        // ENDSAMPLE

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

            using (var conn = new SqlConnection(SqlServerContainer.ConnectionString))
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

            using (var conn = new SqlConnection(SqlServerContainer.ConnectionString))
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

    public class CreateItemHandler
    {
        // SAMPLE: SqlServerOutboxWithSqlTransaction
        [SqlTransaction]
        public async Task<ItemCreatedEvent> Handle(CreateItemCommand command, SqlTransaction tx)
        {
            var item = new Item {Name = command.Name};

            // persist the new Item with the
            // current transaction
            await persist(tx, item);

            return new ItemCreatedEvent {Item = item};
        }
        // ENDSAMPLE

        private Task persist(SqlTransaction tx, Item item)
        {
            // whatever you do to write the new item
            // to your sql server application database
            return Task.CompletedTask;
        }


        public class CreateItemCommand
        {
            public string Name { get; set; }
        }

        public class ItemCreatedEvent
        {
            public Item Item { get; set; }
        }

        public class Item
        {
            public Guid Id;
            public string Name;
        }
    }
}
