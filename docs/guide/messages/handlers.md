# Message Handlers

Jasper purposely eschews the typical `IHandler<T>` approach that most .NET messaging frameworks take in favor of a more flexible
model that relies on naming conventions. This might throw some users that are used to being guided by implementing an expected interface
or base class, but it allows Jasper to be much more flexible and reduces code noise.

As an example, here's about the simplest possible handler you could create:

<!-- snippet: sample_simplest_possible_handler -->
<a id='snippet-sample_simplest_possible_handler'></a>
```cs
public class MyMessageHandler
{
    public void Handle(MyMessage message)
    {
        // do stuff with the message
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/DocumentationSamples/HandlerExamples.cs#L57-L65' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_simplest_possible_handler' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Like most frameworks, Jasper follows the [Hollywood Principle](http://wiki.c2.com/?HollywoodPrinciple) where the framework acts as an intermediary
between the rest of the world and your application code. When a Jasper application receives a `MyMessage` message through one of its transports, Jasper will call your method and pass in the message that it received.

## How Jasper Consumes Your Message Handlers

If you're worried about the performance implications of Jasper calling into your code without any interfaces or base classes, nothing to worry about because Jasper **does not use Reflection at runtime** to call your actions. Instead, Jasper uses [runtime
code generation with Roslyn](https://jeremydmiller.com/2015/11/11/using-roslyn-for-runtime-code-generation-in-marten/) to write the "glue" code around your actions. Internally, Jasper is generating a subclass of `MessageHandler` for each known message type:

<!-- snippet: sample_MessageHandler -->
<a id='snippet-sample_messagehandler'></a>
```cs
public interface IMessageHandler
{
    Task HandleAsync(IMessageContext context, CancellationToken cancellation);
}

public abstract class MessageHandler : IMessageHandler
{
    public HandlerChain? Chain { get; set; }

    public abstract Task HandleAsync(IMessageContext context, CancellationToken cancellation);
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Jasper/Runtime/Handlers/MessageHandler.cs#L6-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_messagehandler' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

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

<!-- snippet: sample_ExampleHandlerByInstance -->
<a id='snippet-sample_examplehandlerbyinstance'></a>
```cs
public class ExampleHandler
{
    public void Handle(Message1 message)
    {
        // Do work synchronously
    }

    public Task Handle(Message2 message)
    {
        // Do work asynchronously
        return Task.CompletedTask;
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/DocumentationSamples/HandlerExamples.cs#L70-L85' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_examplehandlerbyinstance' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Note that you can use either synchronous or asynchronous methods depending on your needs, so you're not constantly being
forced to return `Task.CompletedTask` over and over again for operations that are purely CPU-bound (but Jasper itself might be doing
that for you in its generated `MessageHandler` code).



## Static Handler Methods

<div class="alert alert-info"><b>Note!</b> Using a static method as your message handler can be a small performance
improvement by avoiding the need to create and garbage collect new objects at runtime.</div>

As an alternative, you can also use static methods as message handlers:

<!-- snippet: sample_ExampleHandlerByStaticMethods -->
<a id='snippet-sample_examplehandlerbystaticmethods'></a>
```cs
public static class ExampleHandler
{
    public static void Handle(Message1 message)
    {
        // Do work synchronously
    }

    public static Task Handle(Message2 message)
    {
        // Do work asynchronously
        return Task.CompletedTask;
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/DocumentationSamples/HandlerExamples.cs#L90-L105' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_examplehandlerbystaticmethods' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The handler classes can be static classes as well. This technique gets much more useful when combined with Jasper's
support for method injection in a following section.

## Constructor Injection

Jasper can create your message handler objects by using an IoC container (or in the future just use straight up dependency injection
without any IoC container overhead). In that case, you can happily inject dependencies into your message handler classes through the
constructor like this example that takes in a dependency on an `IDocumentSession` from [Marten](http://jasperfx.github.io/marten):

<!-- snippet: sample_HandlerBuiltByConstructorInjection -->
<a id='snippet-sample_handlerbuiltbyconstructorinjection'></a>
```cs
public class ServiceUsingHandler
{
    private readonly IDocumentSession _session;

    public ServiceUsingHandler(IDocumentSession session)
    {
        _session = session;
    }

    public Task Handle(InvoiceCreated created)
    {
        var invoice = new Invoice {Id = created.InvoiceId};
        _session.Store(invoice);

        return _session.SaveChangesAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/DocumentationSamples/HandlerExamples.cs#L111-L129' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_handlerbuiltbyconstructorinjection' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

See <[linkto:documentation/ioc]> for more information about how Jasper integrates the application's IoC container.

## Method Injection

Similar to ASP.NET MVC Core, Jasper supports the concept of [method injection](https://www.martinfowler.com/articles/injection.html) in handler methods where you can just accept additional
arguments that will be passed into your method by Jasper when a new message is being handled.

Below is an example action method that takes in a dependency on an `IDocumentSession` from [Marten](http://jasperfx.github.io/marten):

<!-- snippet: sample_HandlerUsingMethodInjection -->
<a id='snippet-sample_handlerusingmethodinjection'></a>
```cs
public static class MethodInjectionHandler
{
    public static Task Handle(InvoiceCreated message, IDocumentSession session)
    {
        var invoice = new Invoice {Id = message.InvoiceId};
        session.Store(invoice);

        return session.SaveChangesAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/DocumentationSamples/HandlerExamples.cs#L135-L147' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_handlerusingmethodinjection' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

So, what can be injected as an argument to your message handler?

1. Any service that is registered in your application's IoC container
1. `Envelope`
1. The current time in UTC if you have a parameter like `DateTime now` or `DateTimeOffset now`
1. Services or variables that match a registered code generation strategy. See <[linkto:documentation/execution/middleware_and_codegen]> for more information on this mechanism.

## Cascading Messages from Actions

To have additional messages queued up to be sent out when the current message has been successfully completed,
you can return the outgoing messages from your handler methods with <[linkto:documentation/execution/cascading]>.

## Using the Message Envelope

To access the `Envelope` for the current message being handled in your message handler, just accept `Envelope` as a method
argument like this:

<!-- snippet: sample_HandlerUsingEnvelope -->
<a id='snippet-sample_handlerusingenvelope'></a>
```cs
public class EnvelopeUsingHandler
{
    public void Handle(InvoiceCreated message, Envelope envelope)
    {
        var howOldIsThisMessage =
            DateTimeOffset.Now.Subtract(envelope.SentAt);
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/DocumentationSamples/HandlerExamples.cs#L150-L159' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_handlerusingenvelope' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

See <[linkto:documentation/integration/customizing_envelopes]> for more information on interacting with `Envelope` objects.

## Using the Current IMessageContext

If you want to access or use the current `IMessageContext` for the message being handled to send response messages
or maybe to enqueue local commands within the current outbox scope, just take in `IMessageContext` as a method argument
like in this example:

<!-- snippet: sample_PingHandler -->
<a id='snippet-sample_pinghandler'></a>
```cs
using Jasper;
using Messages;
using Microsoft.Extensions.Logging;

namespace Ponger;

public class PingHandler
{
    public ValueTask Handle(Ping ping, ILogger<PingHandler> logger, IMessageContext context)
    {
        logger.LogInformation("Got Ping #{Number}", ping.Number);
        return context.RespondToSenderAsync(new Pong { Number = ping.Number });
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/PingPong/Ponger/PingHandler.cs#L1-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_pinghandler' title='Start of snippet'>anchor</a></sup>
<a id='snippet-sample_pinghandler-1'></a>
```cs
public static class PingHandler
{
    // Simple message handler for the PingMessage message type
    public static ValueTask Handle(
        // The first argument is assumed to be the message type
        PingMessage message,

        // Jasper supports method injection similar to ASP.Net Core MVC
        // In this case though, IMessageContext is scoped to the message
        // being handled
        IMessageContext context)
    {
        ConsoleWriter.Write(ConsoleColor.Blue, $"Got ping #{message.Number}");

        var response = new PongMessage
        {
            Number = message.Number
        };

        // This usage will send the response message
        // back to the original sender. Jasper uses message
        // headers to embed the reply address for exactly
        // this use case
        return context.RespondToSenderAsync(response);
    }
}
```
<sup><a href='https://github.com/JasperFx/jasper/blob/master/src/Samples/PingPongWithRabbitMq/Ponger/PingHandler.cs#L8-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_pinghandler-1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
