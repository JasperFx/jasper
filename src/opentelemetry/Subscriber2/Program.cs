using Jasper;
using Jasper.RabbitMQ;
using Oakton;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtelMessages;
using Subscriber1;

return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        opts.ServiceName = "Subscriber2";

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
                .AddJasper()
                .AddJaegerExporter();
        });
    })
    .RunOaktonCommands(args);
