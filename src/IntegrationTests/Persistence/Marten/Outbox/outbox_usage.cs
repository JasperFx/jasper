using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests.Persistence.Marten.Persistence;
using IntegrationTests.Persistence.Marten.Persistence.Resiliency;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Persistence.Operations;
using Jasper.Util;
using Marten;
using Servers;
using Servers.Docker;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.Marten.Outbox
{
    public class outbox_usage : MartenContext, IDisposable
    {
        public outbox_usage(DockerFixture<MartenContainer> fixture) : base(fixture)
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

        private readonly JasperRuntime theSender;
        private JasperRuntime theReceiver;
        private readonly MessageTracker theTracker;

        [Fact]
        public async Task basic_send()
        {
            var bus = theSender.Get<IMessageContext>();
            var senderStore = theSender.Get<IDocumentStore>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            using (var session = senderStore.LightweightSession())
            {
                await bus.EnlistInTransaction(session);

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

                var allGone = session.AllIncomingEnvelopes().Any();
                if (!allGone)
                {
                    Thread.Sleep(500);
                    session.AllIncomingEnvelopes().Any().ShouldBeFalse();
                }
            }

            using (var session = senderStore.QuerySession())
            {
                var allGone = session.AllOutgoingEnvelopes().Any();
                if (allGone)
                {
                    Thread.Sleep(500);
                    session.AllOutgoingEnvelopes().Any().ShouldBeFalse();
                }
            }
        }

        [Fact]
        public async Task request_reply_still_works()
        {
            var bus = theSender.Get<IMessageContext>();
            var senderStore = theSender.Get<IDocumentStore>();

            var question = new Question
            {
                X = 3,
                Y = 4
            };

            using (var session = senderStore.LightweightSession())
            {
                await bus.EnlistInTransaction(session);

                var answer = await bus.Request<Answer>(question);
                answer.Sum.ShouldBe(7);
            }
        }

        [Fact]
        public async Task schedule_local_job_durably()
        {
            var bus = theSender.Get<IMessageContext>();
            var senderStore = theSender.Get<IDocumentStore>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            using (var session = senderStore.LightweightSession())
            {
                await bus.EnlistInTransaction(session);

                await bus.Schedule(item, 1.Hours());

                await session.SaveChangesAsync();
            }

            using (var session = senderStore.LightweightSession())
            {
                var scheduled = session.AllIncomingEnvelopes().Single();

                scheduled.MessageType.ShouldBe(typeof(ItemCreated).ToMessageAlias());
                scheduled.Status.ShouldBe(TransportConstants.Scheduled);
            }
        }

        [Fact]
        public async Task send_and_await_still_works()
        {
            var bus = theSender.Get<IMessageContext>();
            var senderStore = theSender.Get<IDocumentStore>();

            var item = new ItemCreated
            {
                Name = "Hat",
                Id = Guid.NewGuid()
            };

            using (var session = senderStore.LightweightSession())
            {
                await bus.EnlistInTransaction(session);

                await bus.SendAndWait(item);
            }

            using (var session = theReceiver.Get<IDocumentStore>().QuerySession())
            {
                var item2 = session.Load<ItemCreated>(item.Id);

                item2.Name.ShouldBe("Hat");

                var deleted = session.AllIncomingEnvelopes().Any();
                if (!deleted)
                {
                    Thread.Sleep(500);
                    session.AllIncomingEnvelopes().Any().ShouldBeFalse();
                }
            }
        }

        [Fact]
        public async Task send_with_receiver_down_to_see_the_persisted_envelopes()
        {
            // Shutting it down
            theReceiver.Dispose();
            theReceiver = null;

            var bus = theSender.Get<IMessageContext>();
            var senderStore = theSender.Get<IDocumentStore>();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            var waiter = theTracker.WaitFor<ItemCreated>();

            using (var session = senderStore.LightweightSession())
            {
                await bus.EnlistInTransaction(session);

                await bus.Send(item);

                await session.SaveChangesAsync();
            }

            using (var session = senderStore.LightweightSession())
            {
                var outgoing = session.AllOutgoingEnvelopes().SingleOrDefault();

                outgoing.MessageType.ShouldBe(typeof(ItemCreated).ToMessageAlias());
            }
        }
    }
}
