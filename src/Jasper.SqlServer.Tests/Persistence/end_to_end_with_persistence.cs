using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.SqlServer.Tests;
using Jasper.SqlServer.Tests.Persistence;
using Jasper.SqlServer.Util;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Persistence
{
    public class end_to_end_with_persistence : IDisposable
    {
        private JasperRuntime theSender;
        private JasperRuntime theReceiver;
        private MessageTracker theTracker;

        public end_to_end_with_persistence()
        {
            theSender = JasperRuntime.For<ItemSender>();
            theReceiver = JasperRuntime.For<ItemReceiver>();

            theTracker = theReceiver.Get<MessageTracker>();

            theSender.RebuildMessageStorage();
            theReceiver.RebuildMessageStorage();


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

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
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


        [Fact]
        public async Task enqueue_locally()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            await theReceiver.Messaging.Enqueue(item);

            waiter.Wait(5.Seconds());

            waiter.IsCompleted.ShouldBeTrue();

            var item2 = await loadItem(item.Id);
            if (item2 == null)
            {
                Thread.Sleep(500);
                item2 = await loadItem(item.Id);
            }



            item2.Name.ShouldBe("Shoe");


            var persistor = theReceiver.Get<SqlServerEnvelopePersistor>();
            if (persistor.AllIncomingEnvelopes().Any())
            {
                await Task.Delay(250.Milliseconds());
                persistor.AllIncomingEnvelopes().Any().ShouldBeFalse();
            }


        }

        [Fact]
        public async Task enqueue_locally_durably()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            await theReceiver.Messaging.EnqueueDurably(item);

            waiter.Wait(5.Seconds());

            waiter.IsCompleted.ShouldBeTrue();

            var item2 = await loadItem(item.Id);
            if (item2 == null)
            {
                Thread.Sleep(500);
                item2 = await loadItem(item.Id);
            }



            item2.Name.ShouldBe("Shoe");

            var persistor = theReceiver.Get<SqlServerEnvelopePersistor>();

            var deleted = persistor.AllIncomingEnvelopes().Any();
            if (!deleted)
            {
                Thread.Sleep(500);
                persistor.AllIncomingEnvelopes().Any().ShouldBeFalse();
            }

        }

        [Fact]
        public async Task send_end_to_end()
        {
            var item = new ItemCreated
            {
                Name = "Hat",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            await theSender.Messaging.Send(item);

            waiter.Wait(5.Seconds());

            waiter.IsCompleted.ShouldBeTrue();


            var persistor = theReceiver.Get<SqlServerEnvelopePersistor>();

            var item2 = await loadItem(item.Id);
            if (item2 == null)
            {
                Thread.Sleep(500);
                item2 = await loadItem(item.Id);
            }



            item2.Name.ShouldBe("Hat");

            if (persistor.AllIncomingEnvelopes().Any())
            {
                await Task.Delay(250.Milliseconds());
                persistor.AllIncomingEnvelopes().Any().ShouldBeFalse();
            }


        }
    }
}
