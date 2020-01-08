<!--title:Marten Backed Message Persistence -->

To use Jasper's version of <[linkto:transports/durable;title=guaranteed delivery with store and forward messaging]> backed by
[Marten](https://jasperfx.github.io/marten) and the [Postgresql database](https://www.postgresql.org/):

1. Install the `Jasper.Marten` library via Nuget
1. Import the `MartenBackedPersistence` extension in your `JasperRegistry` as shown in the code below

<[sample:AppUsingMartenMessagePersistence]>

There's also a shorthand method now that does the equivalent:

<[sample:MartenUsingApp]>

## "Outbox" Pattern Usage

Using the Marten-backed persistence, you can take advantage of Jasper's implementation of the ["outbox" pattern](http://gistlabs.com/2014/05/the-outbox/) where outgoing messages are persisted as part of a native database transaction
before being sent to the outgoing transports. The purpose of this pattern is to achieve guaranteed messaging and consistency
between the outgoing messages and the current transaction without being forced to use distributed, two phase transactions
between your application database and the outgoing queues like [RabbitMQ](https://www.rabbitmq.com/).

To see the outbox pattern in action, consider this [ASP.Net Core MVC controller](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/adding-controller?view=aspnetcore-2.1) action method:

<[sample:using-outbox-with-marten-in-mvc-action]>

A couple notes here:

* The `IMessageContext.EnlistInTransaction(IDocumentSession)` method is an extension method in the `Jasper.Marten` library. When
  it is called, it tells the `IMessageContext` to register any outgoing messages to be persisted by that `IDocumentSession` when
  the Marten session is saved
* No messages will actually be placed into Jasper's outgoing, sender queues until the session is successfully saved
* When the session is saved, the outgoing envelopes will be persisted in the same native Postgresql database, then actually
  sent to the outgoing transport sending agents

Using the outbox pattern, as long as your transaction is successfully committed, the outgoing messages will eventually be sent out, even
if the running system somehow manages to get shut down between the transaction being committed and the messages being successfully
sent to the recipients or even if the recipient services are temporarily down and unreachable.

The outbox usage is a little bit easier to use within a Jasper message handler action decorated with the `[MartenTransaction`] attribute
as shown below:

<[sample:UserHandler-handle-CreateUser]>

By decorating the action with that attribute, `Jasper.Marten` will inject a little bit of code around that method to enlist the current
message context into the current Marten `IDocumentSession`, and the outgoing `UserCreated` message would be persisted as an outgoing envelope when the session is successfully saved.


