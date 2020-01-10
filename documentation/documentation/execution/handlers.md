<!--title:Message Handlers -->

Jasper purposely eschews the typical `IHandler<T>` approach that most .Net messaging frameworks take in favor of a more flexible
model that relies on naming conventions. This might throw some users that are used to being guided by implementing an expected interface
or base class, but it allows Jasper to be much more flexible and reduces code noise.

As an example, here's about the simplest possible handler you could create:

<[sample:simplest-possible-handler]>

Like most frameworks, Jasper follows the [Hollywood Principle](http://wiki.c2.com/?HollywoodPrinciple) where the framework acts as an intermediary
between the rest of the world and your application code. When a Jasper application receives a `MyMessage` message through one of its transports, Jasper will call your method and pass in the message that it received.

## How Jasper Consumes Your Message Handlers

If you're worried about the performance implications of Jasper calling into your code without any interfaces or base classes, nothing to worry about because Jasper **does not use Reflection at runtime** to call your actions. Instead, Jasper uses [runtime
code generation with Roslyn](https://jeremydmiller.com/2015/11/11/using-roslyn-for-runtime-code-generation-in-marten/) to write the "glue" code around your actions. Internally, Jasper is generating a subclass of `MessageHandler` for each known message type:

<[sample:MessageHandler]>

See <[linkto:documentation/execution/handlers]> for information on how Jasper generates the `MessageHandler` code
and how to customize that code.

## Naming Conventions

Out of the box, message handlers need to follow these naming conventions and rules:

* Classes must be public, concrete classes suffixed with either "Handler" or "Consumer"
* Message handling methods must have be public and have a deterministic message type
* The message type has to be a public type

If a candidate method has a single argument, that argument type is assumed to be the message type. Otherwise, Jasper
looks for any argument named either "message", "input", or "@event" to be the message type.

See <[linkto:documentation/execution/discovery]> for more information.

## Instance Handler Methods

Handler methods can be instance methods on handler classes if it's desirable to scope the handler object to the message:

<[sample:ExampleHandlerByInstance]>

Note that you can use either synchronous or asynchronous methods depending on your needs, so you're not constantly being
forced to return `Task.CompletedTask` over and over again for operations that are purely CPU-bound (but Jasper itself might be doing
that for you in its generated `MessageHandler` code).



## Static Handler Methods

<div class="alert alert-info"><b>Note!</b> Using a static method as your message handler can be a small performance
improvement by avoiding the need to create and garbage collect new objects at runtime.</div>

As an alternative, you can also use static methods as message handlers:

<[sample:ExampleHandlerByStaticMethods]>

The handler classes can be static classes as well. This technique gets much more useful when combined with Jasper's
support for method injection in a following section.

## Constructor Injection

Jasper can create your message handler objects by using an IoC container (or in the future just use straight up dependency injection
without any IoC container overhead). In that case, you can happily inject dependencies into your message handler classes through the
constructor like this example that takes in a dependency on an `IDocumentSession` from [Marten](http://jasperfx.github.io/marten):

<[sample:HandlerBuiltByConstructorInjection]>

See <[linkto:documentation/bootstrapping/ioc]> for more information about how Jasper integrates the application's IoC container.

## Method Injection

Similar to ASP.Net MVC Core, Jasper supports the concept of [method injection](https://www.martinfowler.com/articles/injection.html) in handler methods where you can just accept additional
arguments that will be passed into your method by Jasper when a new message is being handled.

Below is an example action method that takes in a dependency on an `IDocumentSession` from [Marten](http://jasperfx.github.io/marten):

<[sample:HandlerUsingMethodInjection]>

So, what can be injected as an argument to your message handler?

1. Any service that is registered in your application's IoC container
1. `Envelope`
1. The current time in UTC if you have a parameter like `DateTime now` or `DateTimeOffset now`
1. Services or variables that match a registered code generation strategy. See <[linkto:documentation/bootstrapping/middleware_and_codegen]> for more information on this mechanism.

## Cascading Messages from Actions

To have additional messages queued up to be sent out when the current message has been successfully completed,
you can return the outgoing messages from your handler methods with <[linkto:documentation/execution/cascading]>.

## Using the Message Envelope

To access the `Envelope` for the current message being handled in your message handler, just accept `Envelope` as a method
argument like this:

<[sample:HandlerUsingEnvelope]>

See <[linkto:documentation/publishing/customizing_envelopes]> for more information on interacting with `Envelope` objects.

