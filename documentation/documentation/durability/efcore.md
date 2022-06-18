<!--title:Using Entity Framework Core with Jasper-->

The `Jasper.Persistence.EntityFrameworkCore` Nuget can be used with a Jasper application to add support for
using [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) in the Jasper:

* `[Transactional]` middleware
* [Outbox](https://microservices.io/patterns/data/transactional-outbox.html) support
* <[linkto:documentation/execution/sagas;title=Saga persistence]>

Note that you will **also** need
to use one of the database backed message persistence mechanisms like <[linkto:documentation/durability/sqlserver;title=Jasper.Persistence.SqlServer]> or <[linkto:documentation/durability/postgresql;title=Jasper.Persistence.Postgresql]> in conjunction with the EF Core integration.

As an example of using the EF Core integration with Sql Server inside a Jasper application,
see the [the InMemoryMediator sample project](https://github.com/JasperFx/JasperSamples/tree/master/InMemoryMediator).

Assuming that `Jasper.Persistence.EntityFrameworkCore` is referenced by your application, here's a custom
[DbContext]() type from the [sample project](https://github.com/JasperFx/JasperSamples/tree/master/InMemoryMediator):

snippet: sample_ItemsDbContext

Most of this is just standard EF Core. The only Jasper specific thing is the call
to `modelBuilder.MapEnvelopeStorage()` in the `OnModelCreating()` method. This adds mappings
to Jasper's <[linkto:documentation/durability;title=message persistence]> and allowing
the `ItemsDbContext` objects to enroll in Jasper outbox transactions.

::: tip warning
You will have to explicitly opt into a specific database persistence for the messaging **and**
also explicitly add in the EF Core transactional support.
:::

Now, to wire up EF Core into our Jasper application and add Sql Server-backed message persistence, use
this <[linkto:documentation/bootstrapping;title=JasperOptions]> class:

snippet: sample_InMemoryMediator_JasperConfig

There's a couple things to note in the code above:

* The call to `Extensions.PersistMessagesWithSqlServer()` sets up the Sql Server backed message persistence
* The `AddDbContext<ItemsDbContext>()` call is just the normal EF Core set up, with one difference. **It's
  a possibly significant performance optimization to mark `optionsLifetime` as singleton scoped** because
  Jasper will be able to generate more efficient handler pipeline code for message handlers that use
  your EF Core `DbContext`.

## Transactional Support and Outbox Usage

First, let's look at using the EF Core-backed outbox usage in the following MVC controller method that:

1. Starts an outbox transaction with `ItemsDbContext` and `IMessageContext` (Jasper's main entrypoint for messaging)
1. Accepts a `CreateItemCommand` command input
1. Creates and saves a new `Item` entity with `ItemsDbContext`
1. Also creates and publishes a matching `ItemCreated` event for any interested subscribers
1. Commits the unit of work
1. Flushes out the newly persisted `ItemCreated` outgoing message to Jasper's sending agents

snippet: sample_InMemoryMediator_DoItAllMyselfItemController

Outside of a Jasper <[linkto:documentation/execution/handlers;title=message handler]>, you will have to explicitly
*enlist* a Jasper `IMessageContext` in a `DbContext` unit of work through the `EnlistInTransaction(DbContext)`
extension method. Secondly, after calling `DbContext.SaveChangesAsync()`, you'll need to manually call
`IMessageContext.SendAllQueuedOutgoingMessages()` to actually release the newly persisted `ItemCreated`
event message to be be sent. If your application somehow manages to crash between the successful call
to `SaveChangesAsync()` and the `ItemCreated` message actually being delivered by Jasper to wherever it
was supposed to go, not to worry. The outgoing message is persisted and will be sent either by restarting
the application or by failing over to another running node of your application.

Alright, that was some busy code, so let's see how this can be cleaner running inside a Jasper message
handler that takes advantage of the `[Transactional]` middleware:

snippet: sample_InMemoryMediator_Items

The code above effectively does the same thing as the `DoItAllMyselfItemController` shown earlier,
but Jasper is generating some middleware code around the `ItemHandler.Handle()` method to enlist
the scoped `IMessageContext` object into the scoped `ItemsDbContext` unit of work. That same middleware is
coming behind the call to `Item.Handler()` and both saving the changes in `ItemsDbContext` and pushing out
any newly persisted cascading messages.




## Saga Persistence

If the `Jasper.Persistence.EntityFrameworkCore` Nuget is referenced, your Jasper application can use custom `DbContext` types
as the <[linkto:documentation/execution/sagas;title=saga persistence mechanism]>. There's just a couple things to know:

1. The primary key / identity for the state document has to be either an `int`, `long`, `string`, or `System.Guid`
1. Jasper analyzes the dependency tree of your `StatefulSagaOf<TState>` handler class for a Lamar dependency that inherits
   from `DbContext`, and if it finds exactly 1 dependency, that is assumed to be used for persisting the state
1. All saga message handlers are automatically wrapped with the transactional middleware
1. You will have to have EF Core mapping for the .NET state type of your saga handler
