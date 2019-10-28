using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Tracking;
using Marten;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Persistence.Testing.Marten.Persistence
{
    public class end_to_end_with_persistence : PostgresqlContext, IDisposable
    {
        private readonly ITestOutputHelper _output;

        public end_to_end_with_persistence(ITestOutputHelper output)
        {
            _output = output;
            theSender = JasperHost.For<ItemSender>();
            theReceiver = JasperHost.For<ItemReceiver>();
            theTracker = theReceiver.Get<MessageTracker>();

            theSender.RebuildMessageStorage();
            theReceiver.RebuildMessageStorage();

        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        private readonly IHost theSender;
        private readonly IHost theReceiver;
        private readonly MessageTracker theTracker;


        [Fact]
        public void can_get_storage_sql()
        {
            var sql = theSender.Get<IEnvelopePersistence>().Admin.CreateSql();

            sql.ShouldNotBeNull();

            _output.WriteLine(sql);
        }

        [Fact]
        public async Task delete_all_persisted_envelopes()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };


            await theSender.Get<IMessagePublisher>().Schedule(item, 1.Days());

            var persistor = theSender.Get<IEnvelopePersistence>();

            var counts = await persistor.Admin.GetPersistedCounts();

            counts.Scheduled.ShouldBe(1);

            persistor.Admin.ClearAllPersistedEnvelopes();

            (await persistor.Admin.GetPersistedCounts()).Scheduled.ShouldBe(0);
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

            await theReceiver.Get<IMessagePublisher>().Enqueue(item);

            waiter.Wait(5.Seconds());

            waiter.IsCompleted.ShouldBeTrue();

            var documentStore = theReceiver.Get<IDocumentStore>();
            using (var session = documentStore.QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);
                if (item2 == null)
                {
                    Thread.Sleep(500);
                    item2 = session.Load<ItemCreated>(item.Id);
                }


                item2.Name.ShouldBe("Shoe");
            }

            var incoming = await theReceiver.Get<IEnvelopePersistence>().Admin.AllIncomingEnvelopes();
            incoming.Any().ShouldBeFalse();
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

            await theReceiver.Get<IMessagePublisher>().EnqueueDurably(item);

            waiter.Wait(5.Seconds());

            waiter.IsCompleted.ShouldBeTrue();

            using (var session = theReceiver.Get<IDocumentStore>().QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);
                if (item2 == null)
                {
                    Thread.Sleep(500);
                    item2 = session.Load<ItemCreated>(item.Id);
                }


                item2.Name.ShouldBe("Shoe");


            }

            var admin = theReceiver.Get<IEnvelopePersistence>().Admin;

            var deleted = (await admin.AllIncomingEnvelopes()).Any();
            if (!deleted)
            {
                Thread.Sleep(500);
                (await admin.AllIncomingEnvelopes()).Any().ShouldBeFalse();
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

            await theSender.Send(item);

            waiter.Wait(20.Seconds());

            waiter.IsCompleted.ShouldBeTrue();

            using (var session = theReceiver.Get<IDocumentStore>().QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);
                if (item2 == null)
                {
                    Thread.Sleep(500);
                    item2 = session.Load<ItemCreated>(item.Id);
                }


                item2.Name.ShouldBe("Hat");

            }

            var admin = theReceiver.Get<IEnvelopePersistence>().Admin;
            (await admin.AllIncomingEnvelopes()).Any().ShouldBeFalse();
        }
    }
}
