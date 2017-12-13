using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Marten.Persistence;
using Jasper.Marten.Tests.Setup;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Lightweight;
using Marten;
using Shouldly;
using Xunit;

namespace Jasper.Marten.Tests.Persistence
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

            var senderStore = theSender.Get<IDocumentStore>();
            senderStore.Advanced.Clean.CompletelyRemoveAll();
            senderStore.Tenancy.Default.EnsureStorageExists(typeof(Envelope));

            var receiverStore = theReceiver.Get<IDocumentStore>();
            receiverStore.Advanced.Clean.CompletelyRemoveAll();
            receiverStore.Tenancy.Default.EnsureStorageExists(typeof(Envelope));


        }

        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        [Fact]
        public async Task send_with_outbox_mechanics()
        {
            var bus = theSender.Get<IServiceBus>();
            var senderStore = theSender.Get<IDocumentStore>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            using (var session = senderStore.LightweightSession())
            {
                bus.EnlistInTransaction(session);

                await bus.Send(item);

                await session.SaveChangesAsync();
            }

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
        public async Task enqueue_locally()
        {
            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            await theReceiver.Bus.Enqueue(item);

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
        public async Task send_end_to_end()
        {
            var item = new ItemCreated
            {
                Name = "Hat",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            await theSender.Bus.Send(item);

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



                item2.Name.ShouldBe("Hat");

                session.AllIncomingEnvelopes().Any().ShouldBeFalse();
            }
        }
    }
}
