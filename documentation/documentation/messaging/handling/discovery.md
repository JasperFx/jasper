<!--title:Message Handler Discovery-->

Jasper has built in mechanisms for automatically finding message handler methods in your application
or the ability to explicitly add handler types. The conventional discovery can
be disabled or customized as well.

## Default Conventional Discovery

Jasper uses [StructureMap 4.0's type scanning support](http://structuremap.github.io/registration/auto-registration-and-conventions/) to find 
handler classes and candidate methods from known assemblies based on naming conventions.

By default, Jasper is looking for public classes in the main application assembly with names matching these rules:

* Type name ends with "Handler"
* Type name ends with "Consumer"

From the types, FubuMVC looks for any public instance method that either accepts a single parameter that is assumed to be the message type, or **one** parameter with one of these names: *message*, *input*, *command*, or *@event*. In addition, 
Jasper will also pick the first parameter as the input type regardless of parameter name if it is concrete, not a "simple" type like a string, date, or number, and not a "Settings" type.

To make that concrete, here are some valid handler method signatures:

<[sample:ValidMessageHandlers]>

## Disabling Conventional Discovery

You can completely turn off any automatic discovery of message handlers through type scanning by
using this syntax in your `JasperRegistry`:

<[sample:ExplicitHandlerDiscovery]>

## Explicitly Ignoring Methods

You can force Jasper to disregard a candidate message handler action at either the class or method
level by using the `[JasperIgnore]` attribute like this:

<[sample:JasperIgnoreAttribute]>


## Customizing Conventional Discovery

<div class="alert alert-warning">Do note that handler finding conventions are additive, meaning that adding additional criteria does
not disable the built in handler discovery</div>

The easiest way to use the Jasper messaging functionality is to just code against the default conventions. However, if you wish to deviate
from those naming conventions you can either supplement the handler discovery or replace it completely with your own conventions.

At a minimum, you can disable the built in discovery, add additional type filtering criteria, or register specific handler classes with the code below:

<[sample:CustomHandlerApp]>


## Subclass or Interface Handlers

Jasper will allow you to use handler methods that work against interfaces or abstract types to apply or reuse
generic functionality across messages. Let's say that some subset of your messages implement some kind of
`IMessage` interface like this one and an implentation of it below:

<[sample:Handlers-IMessage]>

You can handle the `MessageOne` specifically with a handler action like this:

<[sample:Handlers-SpecificMessageHandler]>

You can also create a handler for `IMessage` like this one:

<[sample:Handlers-GenericMessageHandler]>

When Jasper handles the `MessageOne` message, it first calls all the specific handlers for that message type,
then will call any handlers that handle a more generic message type (interface or abstract class most likely) where 
the specific type can be cast to the generic type. You can clearly see this behavior by examining the <[linkto:documentation/diagnostics;title=handler chain diagnostics]>.