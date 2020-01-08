<!--title:RabbitMQ Transport-->

## Quick Start

If you're starting a fresh project, you can quickly spin up a new Jasper project using Rabbit MQ with a `dotnet new` template. 

First install the `JasperTemplates` nuget like so:

```
dotnet new --install JasperTemplates
```

Then build out the directory for your intended project, and use:

```
dotnet new jasper.rabbitmq
```

Then check the `README.md` file in the generated directory for an overview of what was generated for you.

## Installing

To use [RabbitMQ](http://www.rabbitmq.com/) as a transport with Jasper, first install the `Jasper.RabbitMQ` library via nuget to your project. Behind the scenes, this package uses the [RabbitMQ C# Client](https://www.rabbitmq.com/dotnet.html) to both send and receive messages from RabbitMQ.

If you are having any issues with Jasper not correctly auto-discovering the Rabbit MQ transport, you can explicitly register it as shown in this code:

<[sample:AppWithRabbitMq]>

In terms of configuration, there's a few things to worry about:

1. Jasper's concept of Rabbit MQ connection strings, described in the next section
2. Configuring Jasper listeners and publishing rules through Uri values, also described in this document
3. Customizing the Rabbit MQ `ConnectionFactory` (if you need to)
4. Overriding how Jasper maps Envelope properties to Rabbit MQ properties in the case of communicating with a non-Jasper application

## Configuring Rabbit MQ Connections

After installing the `Jasper.RabbitMq` Nuget, the next step is to supply the Rabbit Mq connection string(s) through configuration. You can do that all through
JSON in your appsettings.json file like this sample that is adding a connection string named "MyRabbitConnection":

```
{
    "RabbitMq": {
        "MyRabbitConnection": "Host=RabbitServer;Port=5672;ExchangeType=fanout;ExchangeName=messaging"
    }
}
```

The connection string format is key value pairs similar to an ADO.Net database connection string with the following keys:

* `Host` - the server name. Required.
* `Port` - optional port number for the connection to Rabbit MQ, and the default is 5672.
* `ExchangeName` - optional exchange name. See the [Rabbit MQ documentation](https://www.rabbitmq.com/tutorials/amqp-concepts.html) for more information.
* `ExchangeType` - optional exchange type. The default is 'direct'. Value values are 'direct', 'topic', and 'fanout'. Header exchanges are not supported in Jasper (yet)

Or if you'd like to keep your appsettings.json file structure flat, you can also configure the connection string in your application's `Startup` class like so:

<[sample:Main-activation-of-rabbit-Startup]>

Then build your application with an `IWebHostBuilder` as shown in this example:

<[sample:Main-activation-of-rabbit-with-startup]>

And if you really wanted to for simple proof of concept type or testing applications, you could quickly hard code the Rabbit MQ connection string, listeners,
and related publishing rules as shown in this code:

<[sample:HardCodedRabbitConnection]>



## Uri Structure

Jasper uses Uri values to describe the transport endpoints of both publishing rules and subscriptions. For the Rabbit MQ transport, the base Uri pattern is:

*rabbitmq://[connection string name]/*

where the host of the Uri structure is the named connection string as shown in the previous section. The protocol "rabbitmq" tells Jasper to use the Rabbit MQ transport. The following sections will explain the Uri patterns for queues, subscriptions, and topics.

## Subscribe and Publish Messages to a named Queue

To subscribe or publish to a queue in Rabbit MQ, use the Uri pattern *rabbitmq://[connection string name]/queue/[name of the queue]*. For example, if you strictly use your application's *appsettings.json* file to configure messaging, you can register both a listener to a queue named "incoming" and a rule to publish all messages to a queue named
"outgoing" with a structure like this:

```
{
    "RabbitMq": {
        "MyRabbitConnection": "the connection string to your Rabbit MQ broker and exchange"
    },

    "Jasper": {
        "Listeners": [
            "rabbitmq://rabbit/queue/incoming"
        ],
        "Subscriptions": [
            {
                "Scope": "All",
                "Uri": "rabbitmq://rabbit/queue/outgoing"
            }
        ]
    }
}
```

## Subscribe or Publish Messages for a Topic

You will need to use a Rabbit MQ topic exchange if you wish to publish or subscribe to specific topics. Your connection string might look like:

*Host=server;ExchangeType=topic;ExchangeName=exchange1*

To subscribe or publish messages delivered to a specific topic, you'll need a Uri a topic and subscription name like this:

*rabbitmq://[name of connection string]/topic/[name of topic]*


## Working with Message Specific Topics

You will need to use a Rabbit MQ topic exchange if you wish to publish or subscribe to specific topics. Your connection string might look like:

*Host=server;ExchangeType=topic;ExchangeName=exchange1*

Jasper has a concept of <[linkto:messages;title=message identity]> that can be exploited to automatically publish or subscribe to
topic names matching message types. 

In the case of publishing, the Uri pattern is `rabbitmq://[name of connection string]/topic/*`, where the '*' character is interpreted as meaning the message type name of the message being published. When a message is published to this kind of Uri, Jasper will use the message type name as the topic name.

In the case of subscribing, the Uri pattern is `rabbitmq://[name of connection string]/topic/*`, where the '*' character is interpreted as meaning the message type name of the message being received. Behind the scenes, Jasper is building a separate listening agent for the message type names of all known message types handled by the application.

Below is an example usage:

<[sample:rabbit-MessageSpecificTopicRoutingApp]>


## Fanout Exchanges

For Rabbit MQ fanout exchanges, your connection string would look like this:

*Host=server;ExchangeType=fanout;ExchangeName=exchange1*


## Using Rabbit Mq Routing Key

To publish or subscribe using the Rabbit MQ concept of a "routing key" with direct or fanout exchanges, you declare that in your Uri structure like so:

*rabbitmq://[name of connection string]/queue/[queue name]/routingkey/[name of routing key]*


## Scheduled Messages

Jasper does not at this time support Rabbit MQ's plugin for delayed delivery. When using Rabbit MQ, the scheduled delivery function is done by polling the
configured message store by Jasper rather than depending on Rabbit MQ itself.

See <[linkto:publishing/delayed]> for more information.

## Customizing Rabbit MQ Connections

Jasper tries to allow you to use any and all advanced configuration elements of its underlying transports. To that end, Jasper let's you configure
advanced configuration against the underlying Rabbit MQ `ConnectionFactory` objects for concerns such as authentication as shown below:

<[sample:CustomizedRabbitApp]>


## Connecting to non-Jasper Applications

Lastly, you may want to use the Rabbit MQ transport to integrate with other applications that aren't using Jasper. To make that work, you may need to do some
mapping between Jasper's `Envelope` structure and Rabbit MQ's structures using a custom implementation of `Jasper.RabbitMq.IRabbitMqProtocol`.

That interface is shown below:

<[sample:IRabbitMqProtocol]>

And here's what the default protocol looks like because it's likely easier to start with this than build something all new:

<[sample:DefaultRabbitMqProtocol]>

Lastly, to apply the protocol, use the mechanism shown in the previous section.








