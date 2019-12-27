using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Messaging;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using Jasper.Tracking;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.Durability
{
    public abstract class DurableFixture<TTriggerHandler, TItemCreatedHandler> : Fixture
    {
        private IHost theReceiver;
        private IHost theSender;

        public override void SetUp()
        {

            var receiverPort = PortFinder.FindPort(3340);
            var senderPort = PortFinder.FindPort(3370);

            var publishingUri = $"tcp://localhost:{receiverPort}/durable";


            var senderRegistry = new JasperOptions();
            senderRegistry.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<CascadeReceiver>()
                .IncludeType<ScheduledMessageHandler>();

            senderRegistry.Extensions.UseMessageTrackingTestingSupport();

            senderRegistry.Endpoints.Publish(x =>
            {
                x.Message<TriggerMessage>();
                x.Message<ItemCreated>();
                x.Message<Question>();
                x.Message<ScheduledMessage>();

                x.ToPort(receiverPort).Durably();

            });


            senderRegistry.Endpoints.ListenAtPort(senderPort).Durably();

            configureSender(senderRegistry);

            theSender = JasperHost.For(senderRegistry);
            theSender.RebuildMessageStorage();


            var receiverRegistry = new JasperOptions();
            receiverRegistry.Extensions.UseMessageTrackingTestingSupport();
            receiverRegistry.Handlers.DisableConventionalDiscovery()
                .IncludeType<TTriggerHandler>()
                .IncludeType<TItemCreatedHandler>()
                .IncludeType<QuestionHandler>()
                .IncludeType<ScheduledMessageHandler>();

            receiverRegistry.Endpoints.ListenAtPort(receiverPort).Durably();

            receiverRegistry.Extensions.UseMessageTrackingTestingSupport();

            configureReceiver(receiverRegistry);


            theReceiver = JasperHost.For(receiverRegistry);
            theReceiver.RebuildMessageStorage();


            initializeStorage(theSender, theReceiver);
        }

        private void cleanDatabase()
        {
            initializeStorage(theSender, theReceiver);
            ScheduledMessageHandler.Reset();
        }


        protected abstract void initializeStorage(IHost sender, IHost receiver);

        protected abstract void configureReceiver(JasperOptions receiverOptions);

        protected abstract void configureSender(JasperOptions senderOptions);

        public override void TearDown()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        [FormatAs("Can send a message end to end with a cascading return")]
        public async Task<bool> CanSendMessageEndToEnd()
        {
            cleanDatabase();

            var trigger = new TriggerMessage {Name = Guid.NewGuid().ToString()};

            await theSender
                .TrackActivity()
                .AlsoTrack(theReceiver)
                .WaitForMessageToBeReceivedAt<CascadedMessage>(theSender)
                .SendMessageAndWait(trigger);

            return true;
        }

        protected abstract ItemCreated loadItem(IHost receiver, Guid id);


        protected abstract Task withContext(IHost sender, IMessageContext context,
            Func<IMessageContext, Task> action);

        private Task send(Func<IMessageContext, Task> action)
        {
            return withContext(theSender, theSender.Get<IMessageContext>(), action);
        }

        [FormatAs("Can send items durably through persisted channels")]
        public async Task<bool> CanSendItemDurably()
        {
            cleanDatabase();


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

            StoryTellerAssert.Fail(senderCounts.Outgoing > 0, "There are still persisted, outgoing messages");

            return true;
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

            StoryTellerAssert.Fail(received.Name != item.Name, "The persisted item does not match");
        }

        private async Task assertIncomingEnvelopesIsZero()
        {
            var receiverCounts = await theReceiver.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();
            if (receiverCounts.Incoming > 0)
            {
                await Task.Delay(500.Milliseconds());
                receiverCounts = await theReceiver.Get<IEnvelopePersistence>().Admin.GetPersistedCounts();
            }

            StoryTellerAssert.Fail(receiverCounts.Incoming > 0, "There are still persisted, incoming messages");
        }

        [FormatAs("Can schedule job durably")]
        public async Task<bool> CanScheduleJobDurably()
        {
            cleanDatabase();

            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            await send(async c => { await c.Schedule(item, 1.Hours()); });

            var persistor = theSender.Get<IEnvelopePersistence>();
            var counts = await persistor.Admin.GetPersistedCounts();
            StoryTellerAssert.Fail(counts.Scheduled != 1, $"counts.Scheduled = {counts.Scheduled}, should be 0");


            return true;
        }


        [FormatAs("Can send durably with the receiver down (*must be last*)")]
        public async Task<bool> SendWithReceiverDown()
        {
            cleanDatabase();

            // Shutting it down
            theReceiver.Dispose();
            theReceiver = null;


            var item = new ItemCreated
            {
                Name = "Shoe",
                Id = Guid.NewGuid()
            };

            await send(c => c.Send(item));

            var outgoing = loadAllOutgoingEnvelopes(theSender).SingleOrDefault();

            StoryTellerAssert.Fail(outgoing == null, "No outgoing envelopes are persisted");
            StoryTellerAssert.Fail(outgoing.MessageType != typeof(ItemCreated).ToMessageTypeName(),
                $"Envelope message type expected {typeof(ItemCreated).ToMessageTypeName()}, but was {outgoing.MessageType}");

            return true;
        }

        protected abstract Envelope[] loadAllOutgoingEnvelopes(IHost sender);


        [FormatAs("Can send a scheduled message with durable storage")]
        public async Task<bool> SendScheduledMessage()
        {
            cleanDatabase();

            var message1 = new ScheduledMessage {Id = 1};
            var message2 = new ScheduledMessage {Id = 22};
            var message3 = new ScheduledMessage {Id = 3};

            await send(async c =>
            {
                await c.ScheduleSend(message1, 2.Hours());
                await c.ScheduleSend(message2, 5.Seconds());
                await c.ScheduleSend(message3, 2.Hours());
            });

            ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

            await ScheduledMessageHandler.Received;

            ScheduledMessageHandler.ReceivedMessages.Single()
                .Id.ShouldBe(22);

            return true;
        }

        [FormatAs("Can schedule a local job")]
        public async Task<bool> ScheduleJobLocally()
        {
            cleanDatabase();

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
