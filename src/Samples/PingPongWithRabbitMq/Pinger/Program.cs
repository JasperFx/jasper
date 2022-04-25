using Jasper;
using Jasper.RabbitMQ;
using Oakton;
using Oakton.Resources;
using Pinger;

return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        // Listen for messages coming into the pongs queue
        opts
            .ListenToRabbitQueue("pongs")

            // With the Rabbit MQ transport, you probably
            // want to explicitly designate a specific queue or topic
            // for replies
            .UseForReplies();

        // Publish messages to the pings queue
        opts.PublishAllMessages().ToRabbitQueue("pings");

        // Configure Rabbit MQ connection properties programmatically
        // against a ConnectionFactory
        opts.UseRabbitMq(rabbit =>
        {
            // Using a local installation of Rabbit MQ
            // via a running Docker image
            rabbit.HostName = "localhost";
        });

        opts.PublishAllMessages().ToRabbitQueue("pings");

        // This will send ping messages on a continuous
        // loop
        opts.Services.AddHostedService<PingerService>();

        // This will "auto-provision" all the Rabbit MQ elements
        opts.Services.AddResourceSetupOnStartup();
    }).RunOaktonCommands(args);

