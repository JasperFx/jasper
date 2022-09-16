# Interoperability with Mass Transit

::: info
Jasper has focused first on Rabbit MQ, but other transports will be added soon. The only real difference
is in mapping Jasper's internal Uri scheme to MassTransit's Uri scheme to identify transport endpoints.
:::

Jasper can interoperate bi-directionally with [MassTransit](https://masstransit-project.com/) using [Rabbit MQ](/guides/messaging/transports/masstransit).
At this point, the interoperability is **only** functional if MassTransit is using its standard "envelope" serialization
approach (i.e., **not** using raw JSON serialization).

::: warning
At this point, if an endpoint is set up for interoperability with MassTransit, reserve that endpoint for traffic
with MassTransit, and don't try to use that endpoint for Jasper to Jasper traffic
:::

The configuration to do this is shown below:

<!-- snippet: sample_MassTransit_interoperability -->
<a id='snippet-sample_masstransit_interoperability'></a>
```cs
Jasper = await Host.CreateDefaultBuilder().UseJasper(opts =>
{
    opts.UseRabbitMq()
        .AutoProvision().AutoPurgeOnStartup()
        .BindExchange("jasper").ToQueue("jasper")
        .BindExchange("masstransit").ToQueue("masstransit");

    opts.PublishAllMessages().ToRabbitExchange("masstransit")

        // Tell Jasper to make this endpoint send messages out in a format
        // for MassTransit
        .UseMassTransitInterop();

    opts.ListenToRabbitQueue("jasper")

        // Tell Jasper to make this endpoint interoperable with MassTransit
        .UseMassTransitInterop(mt =>
        {
            // optionally customize the inner JSON serialization
        })
        .DefaultIncomingMessage<ResponseMessage>().UseForReplies();

}).StartAsync();
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/interoperability/InteroperabilityTests/MassTransit/MassTransitSpecs.cs#L24-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_masstransit_interoperability' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


