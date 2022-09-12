using Jasper;
using Jasper.RabbitMQ;
using Jasper.Transports.Tcp;
using Oakton;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtelMessages;
using Subscriber1;

return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        opts.ServiceName = "Subscriber1";

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

        // Add Open Telemetry tracing
        opts.Services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService("Subscriber1"))
                .AddJaegerExporter()

                // Add Jasper as a source
                .AddSource("Jasper");
        });
    })
    .RunOaktonCommands(args);
