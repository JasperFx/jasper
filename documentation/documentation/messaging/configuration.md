<!--title:Configuring the Service Bus-->

All system configuration for a Jasper application starts with the <[linkto:documentation/bootstrapping/configuring_jasper;title=JasperRegistry]> class. Underneath `JasperRegistry` are these sections that are specific to the messaging support:

* `JasperRegistry.Handlers` - Configure policies about how message handlers are discovered, middleware is applied, and error handling policies. See <[linkto:documentation/messaging/handling]> for more information.
* `JasperRegistry.Publish` - Optionally declare what messages or events are published by the Jasper system and any static publishing rules. See <[linkto:documentation/messaging/routing]> for more information.
* `JasperRegistry.Subscribe` - Register as a subscriber for messages from other systems. See <[linkto:documentation/messaging/routing/subscriptions]> for more information.
* `JasperRegistry.Transports` - Configure or disable the built in transports in Jasper. See <[linkto:documentation/messaging/transports]> for more information.

Sample usages of each of these sections are shown below:

<[sample:configuring-messaging-with-JasperRegistry]>

## Listen for Messages

You can direct Jasper to listen for incoming messages with the built in transports by just declaring
the incoming port for the transport or by providing a `Uri` that expresses both the transport type and
port number like this:

<[sample:MyListeningApp]>

Other transport types like the forthcoming RabbitMq and Azure Service Bus transports will probably be configured strictly
by using the `Uri` mechanism.


## Using Strong Typed Configuration

Messaging can also take advantage of Jasper's support for <[linkto:documentation/bootstrapping/configuration;title=strong typed configuration]> like this:

<[sample:configuring-bus-application-with-settings]>


## Uri Lookups and Aliasing

Another way to configure the service bus is to use `Uri` aliasing that allows you to just make the configuration
with "stand in" `Uri's` that are resolved later to the real value. So far, the only mechanism built into the core
Jasper framework will pull the real values from the underlying `IConfigurationRoot` for the application.

See this example below:

<[sample:configuring-via-uri-lookup]>

Behind the scenes, the value for *config://incoming* is interpreted by Jasper as the Uri string stored in configuration with the key "incoming,"
or in code terms, `IConfigurationRoot.GetValue<string>("incoming")`

There is also an addon Uri lookup that uses Consul. See <[linkto:documentation/extensions/consul]> for more information.

The Uri aliases are respected anywhere where `Uri's` are accepted as arguments in `JasperRegistry` or when explicitly specifying the destination
of a message being sent through the messaging.


