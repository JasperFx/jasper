<!--title:Using Marten with Jasper-->

<[info]>
The Jasper.Persistence.Marten has a dependency on the lower level Jasper.Persistence.Postgresql Nuget library.
<[/info]>

The Jasper.Persistence.Marten library provides some easy to use recipes for integrating  [Marten](https://jasperfx.github.io/marten) and Postgresql into a Jasper application. All you need to do to get
started with Marten + Jasper is to add the *Jasper.Persistence.Marten* nuget to your project and at minimum,
at least set the connection string to the underlying Postgresql database by configuring
Marten's `StoreOptions` object like this:

<[sample:AppWithMarten]>

Note that `ConfigureMarten()` is an extension method in Jasper.Marten.

Once that's done, you will be able to inject the following Marten services as either constructor
arguments or method parameters in message or HTTP handlers:

1. `IDocumentStore`
1. `IDocumentSession` - opened with the default `IDocumentStore.OpenSession()` method
1. `IQuerySession`

Likewise, all of these service will be registered in the underlying IoC container for the application.

If you need to customize an `IDocumentSession` for something like transaction levels or automatic dirty checking, we recommend that you just take in `IDocumentStore` and create the session in the application code.

As an example:

<[sample:UsingDocumentSessionHandler]>

## Transactional Middleware

Assuming that the Jasper.Persistence.Marten Nuget is referenced by your project, you can explicitly apply transactional middleware to a message or HTTP handler action with the
`[Transactional]` attribute as shown below:

<[sample:CreateDocCommandHandler]>

Doing this will simply insert a call to `IDocumentSession.SaveChangesAsync()` after the last handler action is called within the generated `MessageHandler`. This effectively makes a unit of work out of all the actions that might be called to process a single message.

This attribute can appear on either the handler class that will apply to all the actions on that class, or on a specific action method.

If so desired, you *can* also use a policy to apply the Marten transaction semantics with a policy. As an example, let's say that you want every message handler where the message type
name ends with "Command" to use the Marten transaction middleware. You could accomplish that
with a handler policy like this:

<[sample:CommandsAreTransactional]>

Then add the policy to your application like this:

<[sample:Using-CommandsAreTransactional]>


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



## Saga Storage

See <[linkto:documentation/execution/sagas]> for an introduction to stateful sagas within Jasper.

To use [Marten](http://jasperfx.github.io/marten) as the backing store for saga persistence, start by enabling
the Marten message persistence like this:

<[sample:SagaApp-with-Marten]>

Any message handlers within a `StatefulSagaOf<T>` class will automatically have the transactional middleware support
applied. The limitation here is that you have to allow Jasper.Marten to handle all the transactional boundaries.

The saga state documents are all persisted as Marten documents.



## Customizing How the Session is Created

By default, using `[Transactional]` or just injecting an `IDocumentSession` with the Marten integration will create a lightweight session in Marten using the `IDocumentStore.LightweightSession()`
call. However, [Marten](http://jasperfx.github.io/marten) has many other options to create sessions
with different transaction levels, heavier identity map behavior, or by attaching custom listeners. To allow you to use the full range of Marten behavior, you can choose to override the mechanics of how
a session is opened for any given message handler by just placing a method called `OpenSession()` on 
your handler class that returns an `IDocumentSession`. If Jasper sees that method exists, it will call that method to create your session. 

Here's an example from the tests:

<[sample:custom-marten-session-creation]>


