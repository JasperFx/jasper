using Jasper;
using Jasper.RabbitMQ;

IHost host = Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        opts
            .ListenToRabbitQueue("pings")

            // With the Rabbit MQ transport, you probably
            // want to explicitly designate a specific queue or topic
            // for replies
            .UseForReplies();

        // Configure Rabbit MQ connections and optionally declare Rabbit MQ
        // objects through an extension method on JasperOptions.Endpoints
        opts.UseRabbitMq().AutoProvision();

        opts.UseRabbitMq();
    })
    .Build();

await host.RunAsync();
