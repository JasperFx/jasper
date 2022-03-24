<!--title: Error Handling-->

::: tip warning
Jasper uses [Polly](https://github.com/App-vNext/Polly) under the covers for the message exception handling with just some custom extension methods for Jasper specific things. You
will be able to use all of Polly's many, many features with Jasper messaging retries.
:::

The sad truth is that Jasper will not unfrequently hit exceptions as it processes messages. In all cases, Jasper will first log the exception using the standard ASP.Net Core `ILogger` abstraction. After that, it walks through the configured error handling policies to
determine what to do next with the message. In the absence of any configured error handling policies,
Jasper will move any message that causes an exception into the error queue for the
transport that the message arrived from on the first attempt.

Today, Jasper has the ability to:

* Enforce a maximum number of attempts to short circuit retries, with the default number being 1
* Selectively apply remediation actions for specific `Exception` types or matching conditions on `Exception` types
* Choose to re-execute the message immediately
* Choose to re-execute the message later
* Re-queue the message for later execution at the back of the line
* Just bail out and move the message out to the error queues
* Discard the message
* Script out how an error is handled on various attempts
* Apply error handling policies globally, by configured policies, or explicitly by chain
* Use custom error handling to do whatever you want utilizing Jasper's `IContinuation` interface





## Configuring Global Error Handling Rules

To establish global error handling policies that apply to all message types, use the syntax as shown below:

snippet: sample_GlobalErrorHandlingConfiguration

In all cases, the global error handling is executed **after** any message type specific error handling.


## Explicit Chain Configuration

To configure specific error handling polices for a certain message (or closely related messages),
you can either use some in the box attributes on the message handler methods as shown below:

snippet: sample_configuring_error_handling_with_attributes

If you prefer -- or have a use case that isn't supported by the attributes, you can take advantage of
Jasper's `Configure(HandlerChain)` convention to do it programmatically. To opt into this, add
a **static** method with the signature `public static void Configure(HandlerChain)` to your handler class
as shown below:

snippet: sample_configure_error_handling_per_chain_with_configure

Do note that if a message handler class handles multiple message types, this method is applied to each
message type chain separately.


## Configuring through Policies

If you want to apply error handling to chains via some kind of policy, you can use an `IHandlerPolicy`
like the one shown below:

snippet: sample_ErrorHandlingPolicy

To apply this policy, use this syntax in your `JasperOptions`:

snippet: sample_MyApp_with_error_handling

## Filtering on Exceptions

To selectively respond to a certain exception type, you have access to all of Polly's exception filtering mechanisms as shown below:

snippet: sample_filtering_by_exception_type

## Built in Error Handling Actions

The most common exception handling actions are shown below:

snippet: sample_continuation_actions

The `RetryLater()` function uses <[linkto:documentation/integration/scheduled]>.

See also <[linkto:documentation/execution/dead_letter_queue]> for more information.

## Scripting Error Handling by Attempt

Using the `TakeActions()` method, you can script out fine-grained retry/requeue/discard policies for an exception
by attempt number as shown below:

<[linkto:AppWithScriptedErrorHandling]>


## Exponential Backoff Policies

By integrating Polly for our retry policies, Jasper gets [exponential backoff](https://en.wikipedia.org/wiki/Exponential_backoff) retry scheduling nearly for free.

To reschedule a message to be retried later at increasingly longer wait times, use this syntax:

snippet: sample_AppWithErrorHandling

## Custom Actions with IContinuation

If you want to write a custom response to failed message handling, you may need to write a custom `IContinuation` that just tells Jasper "what do I do now with this message?":

snippet: sample_IContinuation

Internally, Jasper has built in `IContinuation` strategies for retrying messages, moving messages to the error queue, and requeueing messages among others.

As an example, let's say that on a certain exception type, you want to reschedule the failed message for an hour but also raise some kind of alert event for the support team to know what just happened. A custom continuation class might look like this:

snippet: sample_RaiseAlert_Continuation

Then in usage, we can apply the continuation usage like this:

snippet: sample_AppWithCustomContinuation




