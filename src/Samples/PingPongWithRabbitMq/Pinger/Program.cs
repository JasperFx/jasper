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
        opts.PublishAllMessages().ToRabbit("pings");

        // Configure Rabbit MQ connections and optionally declare Rabbit MQ
        // objects through an extension method on JasperOptions.Endpoints
        opts.ConfigureRabbitMq(rabbit =>
        {
            // Using a local installation of Rabbit MQ
            // via a running Docker image
            rabbit.ConnectionFactory.HostName = "localhost";

            // This just tells Jasper that it might have to create
            // these queues in Rabbit MQ itself
            rabbit.DeclareQueue("pongs");
            rabbit.DeclareQueue("pings");
        });

        opts.PublishAllMessages().ToRabbit("pings");

        // This will send ping messages on a continuous
        // loop
        opts.Services.AddHostedService<PingerService>();

        // This will "auto-provision" all the Rabbit MQ elements
        opts.Services.AddResourceSetupOnStartup();
    }).RunOaktonCommands(args);

