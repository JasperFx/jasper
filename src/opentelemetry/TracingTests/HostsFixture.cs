using Alba;
using Baseline.Dates;
using Jasper;
using Jasper.RabbitMQ;
using Jasper.Transports.Tcp;
using OpenTelemetry.Trace;
using OtelMessages;

namespace TracingTests;

public class HostsFixture : IAsyncLifetime
{
    public IHost FirstSubscriber { get; private set; }
    public IHost SecondSubscriber { get; private set; }
    public IAlbaHost WebApi { get; private set; }

    public async Task InitializeAsync()
    {
        WebApi = await AlbaHost.For<global::Program>(x => { });

        FirstSubscriber = await Host.CreateDefaultBuilder()
            .UseJasper(opts =>
            {
                opts.ServiceName = "Subscriber1";
                opts.ApplicationAssembly = GetType().Assembly;

                opts.Handlers.Discovery(source =>
                {
                    source.DisableConventionalDiscovery();
                    source.IncludeType<Subscriber1Handlers>();
                });

                opts.ListenAtPort(MessagingConstants.Subscriber1Port);

                opts.UseRabbitMq().AutoProvision();

                opts.ListenToRabbitQueue(MessagingConstants.Subscriber1Queue);

                // Publish to the other subscriber
                opts.PublishMessage<RabbitMessage2>().ToRabbitQueue(MessagingConstants.Subscriber2Queue);


                opts.Services.AddOpenTelemetryTracing(builder =>
                {
                    builder
                        .AddJaegerExporter()
                        .AddJasper();
                });

            }).StartAsync();

        SecondSubscriber = await Host.CreateDefaultBuilder()
            .UseJasper(opts =>
            {
                opts.ServiceName = "Subscriber2";
                opts.ApplicationAssembly = GetType().Assembly;

                opts.Handlers.Discovery(source =>
                {
                    source.DisableConventionalDiscovery();
                    source.IncludeType<Subscriber2Handlers>();
                });

                opts.UseRabbitMq().AutoProvision();

                opts.ListenToRabbitQueue(MessagingConstants.Subscriber2Queue);

                // Publish to the same subscriber
                opts.PublishMessage<RabbitMessage3>().ToRabbitQueue(MessagingConstants.Subscriber2Queue);

                opts.Services.AddOpenTelemetryTracing(builder =>
                {
                    builder
                        .AddJaegerExporter()
                        .AddJasper();
                });
            }).StartAsync();
    }

    public async Task DisposeAsync()
    {
        await WebApi.DisposeAsync();
        await FirstSubscriber.StopAsync();
        await SecondSubscriber.StopAsync();
    }
}

public class Subscriber1Handlers
{
    public static async Task Handle(TcpMessage1 cmd, IMessageContext context)
    {
        await Task.Delay(100.Milliseconds());

        await context.RespondToSenderAsync(new TcpMessage2(cmd.Name));
    }

    public static async Task<RabbitMessage2> Handle(RabbitMessage1 message)
    {
        await Task.Delay(100.Milliseconds());
        return new RabbitMessage2 { Name = message.Name };
    }
}

public class Subscriber2Handlers
{
    public static async Task<RabbitMessage3> Handle(RabbitMessage1 message)
    {
        await Task.Delay(50.Milliseconds());
        return new RabbitMessage3{Name = message.Name};
    }

    public static async Task Handle(RabbitMessage3 message, IMessagePublisher publisher)
    {
        await Task.Delay(100.Milliseconds());
        await publisher.EnqueueAsync(new LocalMessage3(message.Name));
    }

    public async Task Handle(LocalMessage3 message, IMessagePublisher publisher)
    {
        await Task.Delay(75.Milliseconds());
        await publisher.EnqueueAsync(new LocalMessage4(message.Name));
    }

    public Task Handle(LocalMessage4 message)
    {
        return Task.Delay(50.Milliseconds());
    }

    public Task Handle(RabbitMessage2 message)
    {
        return Task.Delay(50.Milliseconds());
    }


}
