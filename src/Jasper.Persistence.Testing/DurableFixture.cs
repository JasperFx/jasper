using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Testing.Marten;
using Jasper.Tcp;
using Jasper.Tracking;
using Jasper.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing
{

    public abstract class DurableFixture<TTriggerHandler, TItemCreatedHandler> : IAsyncLifetime
    {
        private IHost theReceiver;
        private IHost theSender;

        public async Task InitializeAsync()
        {
            var receiverPort = PortFinder.GetAvailablePort();
            var senderPort = PortFinder.GetAvailablePort();

            var senderRegistry = new JasperOptions();
            senderRegistry.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<CascadeReceiver>()
                .IncludeType<ScheduledMessageHandler>();

            senderRegistry.Extensions.UseMessageTrackingTestingSupport();

            senderRegistry.Publish(x =>
            {
                x.Message<TriggerMessage>();
                x.Message<ItemCreated>();
                x.Message<Question>();
                x.Message<ScheduledMessage>();

                x.ToPort(receiverPort).Durably();
            });


            senderRegistry.ListenAtPort(senderPort).DurablyPersistedLocally();

            configureSender(senderRegistry);

            theSender = JasperHost.For(senderRegistry);
            await theSender.RebuildMessageStorage();


            var receiverRegistry = new JasperOptions();
            receiverRegistry.Extensions.UseMessageTrackingTestingSupport();
            receiverRegistry.Handlers.DisableConventionalDiscovery()
                .IncludeType<TTriggerHandler>()
                .IncludeType<TItemCreatedHandler>()
                .IncludeType<QuestionHandler>()
                .IncludeType<ScheduledMessageHandler>();

            receiverRegistry.ListenAtPort(receiverPort).DurablyPersistedLocally();

            receiverRegistry.Extensions.UseMessageTrackingTestingSupport();

            configureReceiver(receiverRegistry);


            theReceiver = JasperHost.For(receiverRegistry);
            await theReceiver.RebuildMessageStorage();


            await initializeStorage(theSender, theReceiver);
        }

        public Task DisposeAsync()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();

            return Task.CompletedTask;
        }

        private async Task cleanDatabase()
        {
            await initializeStorage(theSender, theReceiver);
            ScheduledMessageHandler.Reset();
        }


        protected virtual async Task initializeStorage(IHost sender, IHost receiver)
        {
            await theSender.RebuildMessageStorage();

            await theReceiver.RebuildMessageStorage();
        }

        protected abstract void configureReceiver(JasperOptions receiverOptions);

        protected abstract void configureSender(JasperOptions senderOptions);

        [Fact]
        public async Task can_send_message_end_to_end()
        {
            await cleanDatabase();

            var trigger = new TriggerMessage {Name = Guid.NewGuid().ToString()};

            await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                .WaitForMessageToBeReceivedAt<CascadedMessage>(theSender)
                .SendMessageAndWait(trigger);
        }

        protected abstract ItemCreated loadItem(IHost receiver, Guid id);


        protected abstract Task withContext(IHost sender, IExecutionContext context,
            Func<IExecutionContext, Task> action);

        private Task send(Func<IExecutionContext, Task> action)
        {
            return withContext(theSender, theSender.Get<IExecutionContext>(), action);
        }

        [Fact]
        public async Task can_send_items_durably_through_persisted_channels()
        {
            await cleanDatabase();


            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            await theSender.TrackActivity().AlsoTrack(theReceiver).SendMessageAndWait(item);

            await Task.Delay(500.Milliseconds());


            await assertReceivedItemMatchesSent(item);

            await assertIncomingEnvelopesIsZero();


            var senderCounts = await assertNoPersistedOutgoingEnvelopes();

            senderCounts.Outgoing.ShouldBe(0, "There are still persisted, outgoing messages");
        }

        private async Task<PersistedCounts> assertNoPersistedOutgoingEnvelopes()
        {
            var senderCounts = await theSender.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();
            if (senderCounts.Outgoing > 0)
            {
                await Task.Delay(500.Milliseconds());
                senderCounts = await theSender.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();
            }

            return senderCounts;
        }

        private async Task assertReceivedItemMatchesSent(ItemCreated item)
        {
            var received = loadItem(theReceiver, item.Id);
            if (received == null) await Task.Delay(500.Milliseconds());
            received = loadItem(theReceiver, item.Id);

            received.Name.ShouldBe(item.Name, "The persisted item does not match");
        }

        private async Task assertIncomingEnvelopesIsZero()
        {
            var receiverCounts = await theReceiver.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();
            if (receiverCounts.Incoming > 0)
            {
                await Task.Delay(500.Milliseconds());
                receiverCounts = await theReceiver.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();
            }

            receiverCounts.Incoming.ShouldBe(0, "There are still persisted, incoming messages");
        }

        [Fact]
        public async Task can_schedule_job_durably()
        {
            await cleanDatabase();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            await send(async c => { await c.Schedule(item, 1.Hours()); });

            var persistor = theSender.Get<IEnvelopePersistence>();
            var counts = await persistor.Admin.GetPersistedCounts();

            counts.Scheduled.ShouldBe(1, $"counts.Scheduled = {counts.Scheduled}, should be 1");
        }


        protected abstract IReadOnlyList<Envelope> loadAllOutgoingEnvelopes(IHost sender);


        [Fact]
        public async Task<bool> send_scheduled_message()
        {
            await cleanDatabase();

            var message1 = new ScheduledMessage {Id = 1};
            var message2 = new ScheduledMessage {Id = 22};
            var message3 = new ScheduledMessage {Id = 3};

            await send(async c =>
            {
                await c.ScheduleSendAsync(message1, 2.Hours());
                await c.ScheduleSendAsync(message2, 5.Seconds());
                await c.ScheduleSendAsync(message3, 2.Hours());
            });

            ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

            await ScheduledMessageHandler.Received;

            ScheduledMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(22);

            return true;
        }

        [Fact]
        public async Task<bool> schedule_job_locally()
        {
            await cleanDatabase();

            var message1 = new ScheduledMessage {Id = 1};
            var message2 = new ScheduledMessage {Id = 2};
            var message3 = new ScheduledMessage {Id = 3};


            await send(async c =>
            {
                await c.Schedule(message1, 2.Hours());
                await c.Schedule(message2, 5.Seconds());
                await c.Schedule(message3, 2.Hours());
            });


            ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

            await ScheduledMessageHandler.Received;

            ScheduledMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(2);

            return true;
        }


        [Fact]
        public async Task<bool> can_send_durably_with_receiver_down()
        {
            await cleanDatabase();

            // Shutting it down
            theReceiver.Dispose();
            theReceiver = null;


            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            await send(c => c.SendAsync(item));

            var outgoing = loadAllOutgoingEnvelopes(theSender).SingleOrDefault();

            outgoing.ShouldNotBeNull("No outgoing envelopes are persisted");
            outgoing.MessageType.ShouldBe(typeof(ItemCreated).ToMessageTypeName(), $"Envelope message type expected {typeof(ItemCreated).ToMessageTypeName()}, but was {outgoing.MessageType}");

            return true;
        }

    }


    public class TriggerMessage
    {
        public string Name { get; set; }
    }

    public class CascadedMessage
    {
        public string Name { get; set; }
    }

    public class CascadeReceiver
    {
        public void Handle(CascadedMessage message)
        {
        }
    }

    public class ItemCreated
    {
        public Guid Id;
        public string Name;
    }

    public class QuestionHandler
    {
        public Answer Handle(Question question)
        {
            return new Answer
            {
                Sum = question.X + question.Y,
                Product = question.X * question.Y
            };
        }
    }

    public class Question
    {
        public int X;
        public int Y;
    }

    public class Answer
    {
        public int Product;
        public int Sum;
    }

    public class ScheduledMessage
    {
        public int Id { get; set; }
    }

    public class ScheduledMessageHandler
    {
        public static readonly IList<ScheduledMessage> ReceivedMessages = new List<ScheduledMessage>();

        private static TaskCompletionSource<ScheduledMessage> _source;

        public static Task<ScheduledMessage> Received { get; private set; }

        public void Consume(ScheduledMessage message)
        {
            ReceivedMessages.Add(message);
            _source?.TrySetResult(message);
        }

        public static void Reset()
        {
            _source = new TaskCompletionSource<ScheduledMessage>();
            Received = _source.Task;
            ReceivedMessages.Clear();
        }
    }
}
