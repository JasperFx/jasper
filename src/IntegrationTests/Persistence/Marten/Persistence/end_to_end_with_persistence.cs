using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Persistence.Marten.Persistence.Operations;
using Marten;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Persistence.Marten.Persistence
{
    public class end_to_end_with_persistence : MartenContext, IDisposable
    {
        private readonly ITestOutputHelper _output;

        public end_to_end_with_persistence(ITestOutputHelper output)
        {
            _output = output;
            theSender = JasperHost.For<ItemSender>();
            theReceiver = JasperHost.For<ItemReceiver>();
            theTracker = theReceiver.Get<MessageTracker>();

            theSender.Get<IEnvelopePersistor>().Admin.RebuildSchemaObjects();
            theReceiver.Get<IEnvelopePersistor>().Admin.RebuildSchemaObjects();

        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        private readonly IJasperHost theSender;
        private readonly IJasperHost theReceiver;
        private readonly MessageTracker theTracker;


        [Fact]
        public void can_get_storage_sql()
        {
            var sql = theSender.Get<IEnvelopePersistor>().Admin.CreateSql();

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


            await theSender.Messaging.Schedule(item, 1.Days());

            var persistor = theSender.Get<IEnvelopePersistor>();

            var counts = await persistor.GetPersistedCounts();

            counts.Scheduled.ShouldBe(1);

            persistor.Admin.ClearAllPersistedEnvelopes();

            (await persistor.GetPersistedCounts()).Scheduled.ShouldBe(0);
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

            using (var session = theReceiver.Get<IDocumentStore>().QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);
                if (item2 == null)
                {
                    Thread.Sleep(500);
                    item2 = session.Load<ItemCreated>(item.Id);
                }


                item2.Name.ShouldBe("Shoe");

                session.AllIncomingEnvelopes().Any().ShouldBeFalse();
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

            using (var session = theReceiver.Get<IDocumentStore>().QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);
                if (item2 == null)
                {
                    Thread.Sleep(500);
                    item2 = session.Load<ItemCreated>(item.Id);
                }


                item2.Name.ShouldBe("Shoe");

                var deleted = session.AllIncomingEnvelopes().Any();
                if (!deleted)
                {
                    Thread.Sleep(500);
                    session.AllIncomingEnvelopes().Any().ShouldBeFalse();
                }
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

                session.AllIncomingEnvelopes().Any().ShouldBeFalse();
            }
        }
    }
}
