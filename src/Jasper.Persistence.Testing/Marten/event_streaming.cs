using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Persistence.Marten;
using Jasper.Transports.Tcp;
using Jasper.Transports.Util;
using Marten;
using Microsoft.Extensions.Hosting;
using Oakton.Resources;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Persistence.Testing.Marten;

public class event_streaming : PostgresqlContext, IAsyncLifetime
{
    private IHost theReceiver;
    private IHost theSender;

    public async Task InitializeAsync()
    {
        var receiverPort = PortFinder.GetAvailablePort();

        theReceiver = await Host.CreateDefaultBuilder()
            .UseJasper(opts => opts.ListenAtPort(receiverPort))
            .ConfigureServices(services =>
            {
                services.AddMarten(Servers.PostgresConnectionString)
                    .IntegrateWithJasper("receiver");
            }).StartAsync();

        await theReceiver.ResetResourceState();

        theSender = await Host.CreateDefaultBuilder()
            .UseJasper(opts =>
            {
                opts.PublishAllMessages().ToPort(receiverPort).Durably();
                opts.Handlers.DisableConventionalDiscovery().IncludeType<TriggerHandler>();
            })
            .ConfigureServices(services =>
            {
                services.AddMarten(Servers.PostgresConnectionString)
                    .IntegrateWithJasper("sender").EventForwardingToJasper();
            }).StartAsync();

        await theSender.ResetResourceState();
    }

    public async Task DisposeAsync()
    {
        await theReceiver.StopAsync();
        await theSender.StopAsync();
    }

    public void Dispose()
    {
        theReceiver?.Dispose();
        theSender?.Dispose();
    }

    [Fact]
    public async Task event_should_be_published_from_sender_to_receiver()
    {
        var command = new TriggerCommand();

        var waiter = TriggerEventHandler.Waiter;

        await theSender.InvokeAsync(command);

        var @event = await waiter.TimeoutAfterAsync(5000);

        @event.Id.ShouldBe(command.Id);
    }
}

public class TriggerCommand
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

public class TriggerHandler
{
    [Transactional]
    public void Handle(TriggerCommand command, IDocumentSession session)
    {
        session.Events.StartStream(command.Id, new TriggeredEvent{Id = command.Id});
    }
}

public class TriggeredEvent
{
    public Guid Id { get; set; }
}

public class TriggerEventHandler
{
    public static Task<TriggeredEvent> Waiter => _source.Task;

    private static TaskCompletionSource<TriggeredEvent> _source = new();

    public void Handle(TriggeredEvent message)
    {
        _source.SetResult(message);
    }
}
