<!--title:Azure Service Bus Transport-->

<[warning]>
For the moment, Jasper requires all queues and subscriptions in Azure Service Bus to be configured with [sessions enabled](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sessions).
<[/warning]>

## Quick Start

<[info]>
Jasper uses the [Microsoft.Azure.ServiceBus](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus?view=azure-dotnet) Nuget library for accessing Azure Service Bus.
<[/info]>

If you're starting a fresh project, you can quickly spin up a new Jasper project using [Azure Service Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) with a `dotnet new` template. 

First install the `JasperTemplates` nuget like so:

```
dotnet new --install JasperTemplates
```

Then build out the directory for your intended project, and use:

```
dotnet new jasper.azureservicebus
```

Then check the `README.md` file in the generated directory for an overview of what was generated for you.

## Getting Started

All the sample code in this section is [available on GitHub in the sample projects](https://github.com/JasperFx/JasperSamples/tree/master/PingPongWithAzureServiceBus).

The only thing you need to do is to install the `Jasper.AzureServiceBus` Nuget to your Jasper application. This will add the client libraries for Azure Service Bus access and add the transport to your application automatically. 


In terms of configuration, there's a few things to worry about:

1. The Azure Service Bus connection string
2. Configuring Jasper listeners and subscribers
4. Optionally, you can also override how Jasper maps Envelope properties to Azure Service Bus messages in the case of communicating with a non-Jasper application

Here's a sample of a basic ping/pong application that uses Jasper's Azure Service Bus support:

<[sample:JasperConfigForAzureServiceBus]>

For more information about Azure Service Bus connection strings, see [Get started with Service Bus queues](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions).

The Azure Service Bus connection string is the only required configuration, but you can also exert some fine grained control over the 
underlying Azure Service Bus client objects like so:

<[sample:SettingAzureServiceBusOptions]>

If all you care about is the Azure Service Bus connection string, there is an overload shortcut like this example:

<[sample:PublishAndSubscribeToAzureServiceBusQueue]>


## Subscribe and Publish Messages to a named Queue

Below is a sample of configuring both a listener and a publisher endpoint to a named Azure Service Bus queue:

<[sample:PublishAndSubscribeToAzureServiceBusQueue]>

Alternatively, you *could* use `Uri` values instead like so:

<[sample:PublishAndSubscribeToAzureServiceBusQueueByUri]>

Just note that there are some Azure Service Bus specific settings for listeners and senders that won't be exposed through
the generic register by `Uri` mechanisms.

## Subscribe and Publish Messages for a specific Topic

To publish or subscribe to a specific, named Azure Service Bus topic, use the syntax shown below:

<[sample:PublishAndSubscribeToAzureServiceBusTopic]>

Alternatively, you *could* use `Uri` values instead like so:

<[sample:PublishAndSubscribeToAzureServiceBusTopicByUri]>

Just note again that there are some Azure Service Bus specific settings for listeners and senders that won't be exposed through
the generic register by `Uri` mechanisms.

See [Get started with Service Bus topics](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions) for more information;

## Scheduled Messages

Jasper happily uses Azure Service Bus [scheduled messaging](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-sequencing) functionality behind the scenes
when you use Jasper's `ScheduleSend()` functionality. See <[linkto:documentation/integration/scheduled]> for more information.



## Connecting to non-Jasper Applications

Lastly, you may want to use the Azure Service Bus transport to integrate with other applications that aren't using Jasper. To make that work, you may need to do some
mapping between Jasper's `Envelope` structure and Azure Service Bus's `Message` structure using a custom implementation of `Jasper.AzureServiceBus.IAzureServiceBusProtocol`.

That interface is shown below:

<[sample:IAzureServiceBusProtocol]>

And here's what the default protocol looks like because it's likely easier to start with this than build something all new:

<[sample:CustomAzureServiceBusProtocol]>

Lastly, to apply the protocol, use the mechanism shown below:

<[sample:PublishAndSubscribeToAzureServiceBusTopicAndCustomProtocol]>