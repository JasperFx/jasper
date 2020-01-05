<!--title: Message Correlation-->

The `Envelope` class in Jasper tracks several properties for message correlation that you can utilize in your own logging
to track how messages are related to one another:

1. `Id` - The message identifier. This is unique to a message body **and** destination
1. `CausationId` - The identifier to the parent message -- if any -- whose handling triggered the current message
1. `CorrelationId` - Tracks a set of related messages originating from the same original messages. All messages sent from a single `IMessageContext` will share the same `CorrelationId`. In the case of messages resulting from the handling of a first message, the resulting messages will share the same `CorrelationId` as the original `Envelope`.

All of the above properties are of type `Guid`, and the values are assigned through a sequential `Guid` to optimize any database storage of `Envelope` information.

## Customized Message Logging

Jasper internally uses the concept of semantic logging for messaging events with the following interface registered with a default `MessageLogger` implementation in the underlying IoC container.

<[sample:IMessageLogger]>

The default implementation just writes formatted string messages to the built in ASP.Net Core `ILogger` mechanisms. You can of course substitute in your own custom logging to track more structured logging by writing your own custom `IMessageLogger`. The easiest way to do that is to subclass `MessageLogger` and just intercept the events you care about as in this example:

<[sample:CustomMessageLogger]>

Lastly, you can override the `IMessageLogger` in the IoC container like so:

<[sample:AppWithCustomLogging]>