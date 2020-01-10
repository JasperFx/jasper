<!--title:Cascading Messages-->


Many times during the processing of a message you will need to create and send out other messages. Maybe you need to respond back to the original sender with a reply,
maybe you need to trigger a subsequent action, or send out additional messages to start some kind of background processing. You can do that by just having
your handler class use the `IMessageContext` interface as shown in this sample:

<[sample:NoCascadingHandler]>

The code above certainly works and this is consistent with most of the competing service bus tools. However, Jasper supports the concept of _cascading messages_
that allow you to automatically send out objects returned from your handler methods without having to use `IMessageContext` as shown below:

<[sample:CascadingHandler]>

When Jasper executes `CascadingHandler.Consume(MyMessage)`, it "knows" that the `MyResponse` return value should be sent through the 
service bus as part of the same transaction with whatever routing rules apply to `MyResponse`. A couple things to note here:

* Cascading messages returned from handler methods will not be sent out until after the original message succeeds and is part of the underlying
  transport transaction
* Null's returned by handler methods are simply ignored
* The cascading message feature was explicitly designed to make unit testing handler actions easier by shifting the test strategy 
  to [state-based](http://blog.jayfields.com/2008/02/state-based-testing.html) where you mostly need to verify the state of the response
  objects instead of mock-heavy testing against calls to `IMessageContext`.

The response types of your message handlers can be:

1. A specific message type
1. `object`
1. The Jasper `Envelope` if you need to customize how the cascading response is to be sent (schedule send, mark expiration times, route yourself, etc.)
1. `IEnumerable<object>` or `object[]` to make multiple responses
1. A [Tuple](https://docs.microsoft.com/en-us/dotnet/csharp/tuples) type to express the exact kinds of responses your message handler returns


## Request/Reply Scenarios
 
Normally, cascading messages are just sent out according to the configured subscription rules for that message type, but there's
an exception case. If the original sender requested a response, Jasper will automatically send the cascading messages returned
from the action to the original sender if the cascading message type matches the reply that the sender had requested. 
If you're examining the `Envelope` objects for the message, you'll see that the "reply-requested" header
is "MyResponse."

Let's say that we have two running service bus nodes named "Sender" and "Receiver." If this code below
is called from the "Sender" node:

<[sample:Request/Replay-with-cascading]>

and inside Receiver we have this code:

<[sample:CascadingHandler]>

Assuming that `MyMessage` is configured to be sent to "Receiver," the following steps take place:

1. Sender sends a `MyMessage` message to the Receiver node with the "reply-requested" header value of "MyResponse"
1. Receiver handles the `MyMessage` message by calling the `CascadingHandler.Consume(MyMessage)` method
1. Receiver sees the value of the "reply-requested" header matches the response, so it sends the `MyResponse` object back to Sender
1. When Sender receives the matching `MyResponse` message that corresponds to the original `MyMessage`, it sets the completion back
   to the Task returned by the `IMessageContext.Request<TResponse>()` method


## Conditional Responses

You may need some conditional logic within your handler to know what the cascading message is going to be. If you need to return
different types of cascading messages based on some kind of logic, you can still do that by making your handler method return signature
be `object` like this sample shown below:

<[sample:ConditionalResponseHandler]>


## Schedule Response Messages

You may want to raise a delayed or scheduled response. In this case you will need to return an <[linkto:documentation/integration/customizing_envelopes;title=Envelope]> for the response as shown below:

<[sample:DelayedResponseHandler]>

## Multiple Cascading Messages

You can also raise any number of cascading messages by returning either any type that can be
cast to `IEnumerable<object>`, and Jasper will treat each element as a separate cascading message.
An empty enumerable is just ignored.

<[sample:MultipleResponseHandler]>


## Using C# Tuples as Return Values

Sometimes you may well need to return multiple cascading messages from your original message action. In FubuMVC, Jasper's forebear, you had to return either `object[]` or `IEnumerable<object>` as the return type of your action -- which had the unfortunate side effect of partially obfuscating your code by making it less clear what message types were being cascaded from your handler without carefully
reading the message body. In Jasper, we still support the "mystery meat" `object` return value signatures, but now you can also use
C# tuples to better denote the cascading message types.

This handler cascading a pair of messages:

<[sample:MultipleResponseHandler]>

can be rewritten with C# 7 tuples to:

<[sample:TupleResponseHandler]>

The sample above still treats both `GoNorth` and the `ScheduledResponse` as cascading messages. The Jasper team thinks that the
tuple-ized signature makes the code more self-documenting and easier to unit test.
