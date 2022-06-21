using Baseline.Dates;
using Jasper;
using Jasper.RabbitMQ;
using Oakton;

return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        // Going to listen to a queue named "pings", but disregard any messages older than
        // 15 seconds
        opts.ListenToRabbitQueue("pings", queue => queue.TimeToLive(15.Seconds()));

        // Configure Rabbit MQ connections and optionally declare Rabbit MQ
        // objects through an extension method on JasperOptions.Endpoints
        opts.UseRabbitMq() // This is short hand to connect locally
            .DeclareExchange("pings", exchange =>
            {
                // Also declares the queue too
                exchange.BindQueue("pings");
            })
            .AutoProvision()

            // Option to blow away existing messages in
            // all queues on application startup
            .AutoPurgeOnStartup();
    })
    .RunOaktonCommands(args);
