using Alba;
using Jasper;
using Jasper.RabbitMQ;
using Jasper.Transports.Tcp;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtelMessages;
using Subscriber1;

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
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("Subscriber2").AddTelemetrySdk())
                        .AddJaegerExporter();
                });
            }).StartAsync();
    }

    public async Task DisposeAsync()
    {
        // await WebApi.DisposeAsync();
        // await FirstSubscriber.StopAsync();
        await SecondSubscriber.StopAsync();
    }
}
