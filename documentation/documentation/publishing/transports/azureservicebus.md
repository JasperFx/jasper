<!--title:Azure Service Bus Transport-->

<[warning]>
For the moment, Jasper requires all queues and subscriptions in Azure Service Bus to be configured with [sessions enabled](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sessions).
<[/warning]>

## Quick Start

If you're starting a fresh project, you can quickly spin up a new Jasper project using Azure Service Bus with a `dotnet new` template. 

First install the `JasperTemplates` nuget like so:

```
dotnet new --install JasperTemplates
```

Then build out the directory for your intended project, and use:

```
dotnet new jasper.azureservicebus
```

Then check the `README.md` file in the generated directory for an overview of what was generated for you.

## Installing

The only thing you need to do is to install the `Jasper.AzureServiceBus` Nuget to your Jasper application. This will add the client libraries for Azure Service Bus access
and add the transport to your application automatically. 

If Jasper is not auto-discovering the Azure Service Bus, you can also explicitly register the transport as shown below:

<[sample:AppWithAzureServiceBus]>


In terms of configuration, there's a few things to worry about:

1. Azure Service Bus connection strings, described in the next section
2. Configuring Jasper listeners and publishing rules through Uri values, also described in this document
3. Customizing the Azure Service Bus clients
4. Overriding how Jasper maps Envelope properties to Azure Service Bus messages in the case of communicating with a non-Jasper application

## Configuring Azure Bus Service Connections

After installing the `Jasper.AzureServiceBus` Nuget, the next step is to supply the Azure Service Bus connection string(s) through configuration. You can do that all through
JSON in your appsettings.json file like this sample that is adding a connection string named "MyAsbConnection":

```
{
    "AzureServiceBus": {
        "MyAsbConnection": "the connection string to your ASB namespace"
    }
}
```

Or if you'd like to keep your appsettings.json file structure flat, you can also configure the connection string in your application's `Startup` class like so:

<[sample:Main-activation-of-asb-Startup]>

Then build your application with an `IWebHostBuilder` as shown in this example:

<[sample:Main-activation-of-asb-with-startup]>

And if you really wanted to for simple proof of concept type or testing applications, you could quickly hard code the Azure Service Bus connection string, listeners,
and related publishing rules as shown in this code:

<[sample:HardCodedASBConnection]>


For more information about Azure Service Bus connection strings, see [Get started with Service Bus queues](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions).


## Uri Structure

Jasper uses Uri values to describe the transport endpoints of both publishing rules and subscriptions. For the Azure Service Bus transport, the base Uri pattern is:

*azureservicebus://[connection string name]/*

where the host of the Uri structure is the named connection string as shown in the previous section. The protocol "azureservicebus" tells Jasper to use the Azure Service Bus transport. The following sections will explain the Uri patterns for queues, subscriptions, and topics.

## Subscribe and Publish Messages to a named Queue

To subscribe or publish to a queue in Azure Service Bus, use the Uri pattern *azureservicebus://[connection string name]/queue/[name of the queue]*. For example, if you strictly use your application's *appsettings.json* file to configure messaging, you can register both a listener to a queue named "incoming" and a rule to publish all messages to a queue named
"outgoing" with a structure like this:

```
{
    "AzureServiceBus": {
        "MyAsbConnection": "the connection string to your ASB namespace"
    },

    "Jasper": {
        "Listeners": [
            "azureservicebus://azure/queue/incoming"
        ],
        "Subscriptions": [
            {
                "Scope": "All",
                "Uri": "azureservicebus://azure/queue/outgoing"
            }
        ]
    }
}
```



## Subscribe to Messages for a Topic

To subscribe to messages delivered to a specific topic, you'll need a Uri with both a topic and subscription name like this:

*azureservicebus://[name of connection string]/subscription/[name of subscription]/topic/[name of topic]*

Do note that both subscription and topic are required, but could be designated in any order.

## Publish Messages for a Topic

To simply publish messages to a topic, you only need to specify the topic name and connection name like this:

*azureservicebus://[name of connection string]/topic/[name of topic]*

## Working with Message Specific Topics

Jasper has a concept of <[linkto:documentation/messaging/messages;title=message identity]> that can be exploited to automatically publish or subscribe to
topic names matching message types. 

In the case of publishing, the Uri pattern is `azureservicebus://[name of connection string]/topic/*`, where the '*' character is interpreted as meaning the message type name of the message being published. When a message is published to this kind of Uri, Jasper will use the message type name as the topic name.

In the case of subscribing, the Uri pattern is `azureservicebus://[name of connection string]/subscription/myapp/topic/*`, where the '*' character is interpreted as meaning the message type name of the message being received. Behind the scenes, Jasper is building a separate listening agent for the message type names of all known message types handled by the application.

Below is an example usage:

<[sample:asb-MessageSpecificTopicRoutingApp]>

## Scheduled Messages

Jasper happily uses Azure Service Bus [scheduled messaging](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing) functionality behind the scenes
when you use Jasper's `ScheduleSend()` functionality. See <[linkto:documentation/messaging/publishing/delayed]> for more information.

## Customizing Clients

Jasper tries to allow you to use any and all advanced configuration elements of its underlying transports. To that end, Jasper let's you configure
advanced configuration against the underlying Azure Service Bus `Client` objects as shown below:

<[sample:CustomizedAzureServiceBusApp]>


## Connecting to non-Jasper Applications

Lastly, you may want to use the Azure Service Bus transport to integrate with other applications that aren't using Jasper. To make that work, you may need to do some
mapping between Jasper's `Envelope` structure and Azure Service Bus's `Message` structure using a custom implementation of `Jasper.AzureServiceBus.IAzureServiceBusProtocol`.

That interface is shown below:

<[sample:IAzureServiceBusProtocol]>

And here's what the default protocol looks like because it's likely easier to start with this than build something all new:

<[sample:DefaultAzureServiceBusProtocol]>

Lastly, to apply the protocol, use the mechanism shown in the previous section.