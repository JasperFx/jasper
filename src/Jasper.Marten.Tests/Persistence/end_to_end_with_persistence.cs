using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Marten.Persistence;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Tests.Setup;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging;
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
