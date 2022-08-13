using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Tracking;
using Jasper.Transports.Tcp;
using Jasper.Util;
using Lamar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Oakton.Resources;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing;

public abstract class DurabilityComplianceContext<TTriggerHandler, TItemCreatedHandler> : IAsyncLifetime
{
    private IHost theReceiver;
    private IHost theSender;

    public async Task InitializeAsync()
    {
        var receiverPort = PortFinder.GetAvailablePort();
        var senderPort = PortFinder.GetAvailablePort();


        var senderRegistry = new JasperOptions();
        senderRegistry.Services.ForSingletonOf<ILogger>().Use(NullLogger.Instance);
        senderRegistry.Handlers
            .DisableConventionalDiscovery()
            .IncludeType<CascadeReceiver>()
            .IncludeType<ScheduledMessageHandler>();


        senderRegistry.Publish(x =>
        {
            x.Message<TriggerMessage>();
            x.Message<ItemCreated>();
            x.Message<Question>();
            x.Message<ScheduledMessage>();

            x.ToPort(receiverPort).UsePersistentOutbox();
        });


        senderRegistry.ListenAtPort(senderPort).UsePersistentInbox();

        configureSender(senderRegistry);

        theSender = JasperHost.For(senderRegistry);


        var receiverRegistry = new JasperOptions();
        receiverRegistry.Services.ForSingletonOf<ILogger>().Use(NullLogger.Instance);
        receiverRegistry.Handlers.DisableConventionalDiscovery()
            .IncludeType<TTriggerHandler>()
            .IncludeType<TItemCreatedHandler>()
            .IncludeType<QuestionHandler>()
            .IncludeType<ScheduledMessageHandler>();

        receiverRegistry.ListenAtPort(receiverPort).UsePersistentInbox();

        configureReceiver(receiverRegistry);


        theReceiver = JasperHost.For(receiverRegistry);

        await theSender.ResetResourceState();
        await theReceiver.ResetResourceState();
    }

    protected abstract void configureReceiver(JasperOptions receiverRegistry);

    protected abstract void configureSender(JasperOptions senderRegistry);


    public async Task DisposeAsync()
    {
        await theReceiver.StopAsync();
        await theSender.StopAsync();
    }

    [Fact]
    public async Task<bool> CanSendMessageEndToEnd()
    {
        await cleanDatabase();

        var trigger = new TriggerMessage { Name = Guid.NewGuid().ToString() };

        await theSender
            .TrackActivity()
            .AlsoTrack(theReceiver)
            .WaitForMessageToBeReceivedAt<CascadedMessage>(theSender)
            .SendMessageAndWaitAsync(trigger);

        return true;
    }

    private async ValueTask cleanDatabase()
    {
        await theReceiver.ResetResourceState();
        await theSender.ResetResourceState();
    }

    protected abstract ItemCreated loadItem(IHost receiver, Guid id);


    protected abstract Task withContext(IHost sender, IExecutionContext context,
        Func<IExecutionContext, ValueTask> action);

    private async Task send(Func<IExecutionContext, ValueTask> action)
    {
        var container = theSender.Services.As<IContainer>();
        using (var nested = container.GetNestedContainer())
        {
            await withContext(theSender, nested.GetInstance<IExecutionContext>(), action);
        }
    }

    [Fact]
    public async Task<bool> CanSendItemDurably()
    {
        await cleanDatabase();


        var item = new ItemCreated
        {
            Name = "Shoe",
            Id = Guid.NewGuid()
        };

        await theSender.TrackActivity().AlsoTrack(theReceiver).SendMessageAndWaitAsync(item);

        await Task.Delay(500.Milliseconds());

        await assertReceivedItemMatchesSent(item);

        await assertIncomingEnvelopesIsZero();

        var senderCounts = await assertNoPersistedOutgoingEnvelopes();

        senderCounts.Outgoing.ShouldBeGreaterThan(0);

        return true;
    }

    private async Task<PersistedCounts> assertNoPersistedOutgoingEnvelopes()
    {
        var senderCounts = await theSender.Get<IEnvelopePersistence>().Admin.FetchCountsAsync();
        if (senderCounts.Outgoing > 0)
        {
            await Task.Delay(500.Milliseconds());
            senderCounts = await theSender.Get<IEnvelopePersistence>().Admin.FetchCountsAsync();
        }

        return senderCounts;
    }

    private async Task assertReceivedItemMatchesSent(ItemCreated item)
    {
        var received = loadItem(theReceiver, item.Id);
        if (received == null)
        {
            await Task.Delay(500.Milliseconds());
        }

        received = loadItem(theReceiver, item.Id);

        received.Name.ShouldBe(item.Name);

    }

    private async Task assertIncomingEnvelopesIsZero()
    {
        var receiverCounts = await theReceiver.Get<IEnvelopePersistence>().Admin.FetchCountsAsync();
        if (receiverCounts.Incoming > 0)
        {
            await Task.Delay(500.Milliseconds());
            receiverCounts = await theReceiver.Get<IEnvelopePersistence>().Admin.FetchCountsAsync();
        }

        receiverCounts.Incoming.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task<bool> CanScheduleJobDurably()
    {
        await cleanDatabase();

        var item = new ItemCreated
        {
            Name = "Shoe",
            Id = Guid.NewGuid()
        };

        await send(async c => { await c.ScheduleAsync(item, 1.Hours()); });

        var persistence = theSender.Get<IEnvelopePersistence>();
        var counts = await persistence.Admin.FetchCountsAsync();
        counts.Scheduled.ShouldBe(0);

        return true;
    }


    [Fact]
    public async Task<bool> SendWithReceiverDown()
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

        outgoing.ShouldNotBeNull();
        outgoing.MessageType.ShouldBe(typeof(ItemCreated).ToMessageTypeName());

        return true;
    }

    protected abstract IReadOnlyList<Envelope> loadAllOutgoingEnvelopes(IHost sender);


    [Fact]
    public async Task<bool> SendScheduledMessage()
    {
        await cleanDatabase();

        var message1 = new ScheduledMessage { Id = 1 };
        var message2 = new ScheduledMessage { Id = 22 };
        var message3 = new ScheduledMessage { Id = 3 };

        await send(async c =>
        {
            await c.ScheduleAsync(message1, 2.Hours());
            await c.ScheduleAsync(message2, 5.Seconds());
            await c.ScheduleAsync(message3, 2.Hours());
        });

        ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

        await ScheduledMessageHandler.Received;

        ScheduledMessageHandler.ReceivedMessages.Single()
            .Id.ShouldBe(22);

        return true;
    }

    [Fact]
    public async Task<bool> ScheduleJobLocally()
    {
        await cleanDatabase();

        var message1 = new ScheduledMessage { Id = 1 };
        var message2 = new ScheduledMessage { Id = 2 };
        var message3 = new ScheduledMessage { Id = 3 };


        await send(async c =>
        {
            await c.ScheduleAsync(message1, 2.Hours());
            await c.ScheduleAsync(message2, 5.Seconds());
            await c.ScheduleAsync(message3, 2.Hours());
        });


        ScheduledMessageHandler.ReceivedMessages.Count.ShouldBe(0);

        await ScheduledMessageHandler.Received;

        ScheduledMessageHandler.ReceivedMessages.Single()
            .Id.ShouldBe(2);

        return true;
    }
}




