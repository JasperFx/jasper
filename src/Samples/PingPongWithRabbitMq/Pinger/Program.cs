using System.Net.NetworkInformation;
using Jasper;
using Jasper.RabbitMQ;
using Oakton;
using Pinger;

return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        // Listen for messages coming into the pongs queue
        opts
            .ListenToRabbitQueue("pongs")

            // This won't be necessary by the time Jasper goes 2.0
            // but for now, I've got to help Jasper out a little bit
            .UseForReplies();

        // Publish messages to the pings queue
        opts.PublishMessage<PingMessage>().ToRabbitExchange("pings");

        // Configure Rabbit MQ connection properties programmatically
        // against a ConnectionFactory
        opts.UseRabbitMq(rabbit =>
        {
            // Using a local installation of Rabbit MQ
            // via a running Docker image
            rabbit.HostName = "localhost";
        })
            // Directs Jasper to build any declared queues, exchanges, or
            // bindings with the Rabbit MQ broker as part of bootstrapping time
            .AutoProvision();

        // This will send ping messages on a continuous
        // loop
        opts.Services.AddHostedService<PingerService>();
    }).RunOaktonCommands(args);

