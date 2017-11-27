<!--title: Error Handling-->


The sad truth is that JasperBus will not unfrequently hit exceptions as it processes messages. In all cases, JasperBus will first log the exception using the <[linkto:documentation/messaging/logging;title=the IBusLogger sinks]>. After that, it walks through the configured error handling policies to
determine what to do next with the message. In the absence of any confugured error handling policies,
JasperBus will move any message that causes an exception into the error queue for the
transport that the message arrived from on the first attempt.

Today, JasperBus has the ability to:

* Enforce a maximum number of attempts to short circuit retries, with the default number being 1
* Selectively apply remediation actions for a given type of `Exception` 
* Choose to re-execute the message immediately
* Choose to re-execute the message later
* Just bail out and move the message out to the error queues
* Apply error handling policies globally, by configured policies, or explicitly by chain


## Configuring Global Error Handling Rules

To establish global error handling policies that apply to all message types, use the 
`JasperBusRegistry.ErrorHandling` syntax as shown below:

<[sample:GlobalErrorHandlingConfiguration]>

In all cases, the global error handling is executed **after** any message type specific error handling.


## Explicit Chain Configuration

To configure specific error handling polices for a certain message (or closely related messages),
you can either use some in the box attributes on the message handler methods as shown below:

<[sample:configuring-error-handling-with-attributes]>

If you prefer -- or have a use case that isn't supported by the attributes, you can take advantage of
JasperBus's `Configure(HandlerChain)` convention to do it programmatically. To opt into this, add
a **static** method with the signature `public static void Configure(HandlerChain)` to your handler class
as shown below:

<[sample:configure-error-handling-per-chain-with-configure]>

Do note that if a message handler class handles multiple message types, this method is applied to each
message type chain separately.


## Configuring through Policies

If you want to apply error handling to chains via some kind of policy, you can use an `IHandlerPolicy`
like the one shown below:

<[sample:ErrorHandlingPolicy]>

To apply this policy, use this syntax in your `JasperBusRegistry`:

<[sample:MyApp-with-error-handling]>

## Filtering on Exceptions

To selectively respond to a certain exception type, you have these two built in methods to configure
the error handling filtering:

<[sample:filtering-by-exception-type]>

## Built in Error Handling Actions

<div class="alert alert-warning">Delayed message processing is not yet supported in Jasper.</div>


The most common exception handling actions are shown below:

<[sample:continuation-actions]>


## Raise Other Messages

You can also choose to send additional messages as a result of the exception. In this case, you can
use the original message and the `Envelope` metadata plus the actual `Exception` to determine the
message(s) to send out. Use this capability if you want to notify senders when a message fails.

<[sample:RespondWithMessages]>

** TODO -- talk about how to send it back to the original sender **

## Custom Error Handlers

If the built in recipes don't cover your exception handling needs, all isn't lost. You can bypass
the helpers and write your own class that implements the `IErrorHandler` interface shown below:

<[sample:IErrorHandler]>

Your error handler needs to be able to look at an `Exception` and `Envelope`, then determine and return
an `IContinuation` object that will be executed against the current message. That interface is shown below:

<[sample:IContinuation]>

Here's an example of a custom error handler:

<[sample:CustomErrorHandler]>

To register this custom error handler with your application, just add it to the `ErrorHandlers` collection:

<[sample:Registering-CustomErrorHandler]>

Note that you can apply custom error handlers either globally or by message type.

TODO -- link to the docs on using IEnvelopeContext & Envelope, when they actually exist;)
