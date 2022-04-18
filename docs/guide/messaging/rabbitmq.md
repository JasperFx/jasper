# With Rabbit MQ

::: tip
Jasper uses the [Rabbit MQ .Net Client](https://www.rabbitmq.com/dotnet.html) to connect to Rabbit MQ.
:::

To use Rabbit MQ as a Jasper transport, first install the [Jasper.RabbitMq](https://www.nuget.org/packages/Jasper.RabbitMQ/) Nuget dependency to your project like so:

```bash
dotnet add package Jasper.RabbitMQ
```

## Installing

All the code samples in this section are from the [Ping/Pong with Rabbit MQ sample project](https://github.com/JasperFx/JasperSamples/tree/master/PingPongWithRabbitMQ).

To use [RabbitMQ](http://www.rabbitmq.com/) as a transport with Jasper, first install the `Jasper.RabbitMQ` library via nuget to your project. Behind the scenes, this package uses the [RabbitMQ C# Client](https://www.rabbitmq.com/dotnet.html) to both send and receive messages from RabbitMQ.

snippet: sample_BootstrappingRabbitMQ

See the [Rabbit MQ .Net Client documentation](https://www.rabbitmq.com/dotnet-api-guide.html#connecting) for more information about configuring the `ConnectionFactory` to connect to Rabbit MQ.

All the calls to `Declare*****()` are optional helpers for auto-provisioning Rabbit MQ objects on application startup. This is probably only useful for development or testing, but it's there.


## Subscribe and Publish Messages to a named Queue or Routing Key

In terms of publishing or listening to a specific, named queue (or publish to a routing key), use the syntax shown below:

snippet: sample_PublishAndListenForRabbitMqQueue

Or if you want to do this by `Uri`:

snippet: sample_PublishAndListenForRabbitMqQueueByUri

Please note that you will lose the option to configure Rabbit MQ-specific options by endpoint if you use the generic
`Uri` approach.

## Publish Messages to a specific Topic

Publishing to a specific topic can be done with this syntax:

snippet: sample_PublishRabbitMqTopic

**Please note** that in the call to `Endpoints.Publish****().ToRabbitMq()`, the second argument refers to the Rabbit MQ exchange name
and this must be specified to publish to a named topic.

## Fanout Exchanges

Jasper.RabbitMQ supports using [Rabbit MQ *fanout* exchanges](https://www.tutlane.com/tutorial/rabbitmq/csharp-rabbitmq-fanout-exchange) as shown below:

snippet: sample_PublishRabbitMqFanout

## Scheduled Messages

Jasper does not at this time support Rabbit MQ's plugin for delayed delivery. When using Rabbit MQ, the scheduled delivery function is done by polling the
configured message store by Jasper rather than depending on Rabbit MQ itself.

See <[linkto:documentation/integration/scheduled]> for more information.


## Connecting to non-Jasper Applications

Lastly, you may want to use the Rabbit MQ transport to integrate with other applications that aren't using Jasper. To make that work, you may need to do some
mapping between Jasper's `Envelope` structure and Rabbit MQ's structures using a custom implementation of `Jasper.RabbitMq.IRabbitMqProtocol`.

That interface is shown below:

snippet: sample_IRabbitMqProtocol

And here's what the default protocol looks like because it's likely easier to start with this than build something all new:

snippet: sample_DefaultRabbitMqProtocol

Lastly, to apply the protocol, use the mechanism shown in the previous section.


## Auto-Provisioning Rabbit MQ Objects at Application Startup

One of the best things about developing against Rabbit MQ is how easy it is to set up your environment for local development. Run Rabbit MQ in a Docker container and spin up queues, exchanges, and bindings on the fly. Jasper.RabbitMQ uses a combination of the `Declare***()` methods and the `AutoProvision` property to direct Jasper to create any missing Rabbit MQ objects at application start up time.

Here's a sample usage:

snippet: sample_PublishRabbitMqTopic

