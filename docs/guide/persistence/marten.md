# With Marten

[Marten](https://martendb.io) and Jasper are sibling projects under the [JasperFx organization](https://github.com/jasperfx), and as such, have quite a bit of synergy when
used together. At this point, adding the *Jasper.Persistence.Marten* Nuget dependency to your application adds the capability to combine Marten and Jasper to:

* Simplify persistent handler coding with transactional middleware
* Use Marten and Postgresql as a persistent inbox or outbox with Jasper messaging
* Support persistent sagas within Jasper applications
* Effectively use Jasper and Marten together for a [Decider](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider) function workflow with event sourcing
* Selectively publish events captured by Marten through Jasper messaging

## Getting Started

To use the Jasper integration with Marten, just install the Jasper.Persistence.Marten Nuget into your application. Assuming that you've [configured Marten](https://martendb.io/configuration/)
in your application (and Jasper itself!), you next need to add the Jasper integration to Marten as shown in this sample application bootstrapping:

<!-- snippet: sample_integrating_jasper_with_marten -->
<a id='snippet-sample_integrating_jasper_with_marten'></a>
```cs
var builder = WebApplication.CreateBuilder(args);
builder.Host.ApplyOaktonExtensions();

builder.Services.AddMarten(opts =>
    {
        var connectionString = builder
            .Configuration
            .GetConnectionString("postgres");

        opts.Connection(connectionString);
        opts.DatabaseSchemaName = "orders";
    })
    // Optionally add Marten/Postgresql integration
    // with Jasper's outbox
    .IntegrateWithJasper();

    // You can also place the Jasper database objects
    // into a different database schema, in this case
    // named "jasper_messages"
    //.IntegrateWithJasper("jasper_messages");

builder.Host.UseJasper(opts =>
{
    // I've added persistent inbox
    // behavior to the "important"
    // local queue
    opts.LocalQueue("important")
        .UsePersistentInbox();
});
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Program.cs#L9-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_integrating_jasper_with_marten' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

TODO -- link to the outbox page
TODO -- link to the sample code project

Using the `IntegrateWithJasper()` extension method behind your call to `AddMarten()` will:

* Register the necessary [inbox and outbox](/guide/persistence/) database tables with [Marten's database schema management](https://martendb.io/schema/migrations.html)
* Adds Jasper's "DurabilityAgent" to your .NET application for the inbox and outbox
* Makes Marten the active [saga storage](/guide/persistence/sagas) for Jasper
* Adds transactional middleware using Marten to your Jasper application


## Marten as Outbox

::: tip
Jasper's outbox will help you order all outgoing messages until after the database transaction succeeds, but only messages being delivered
to endpoints explicitly configured to be persistent will be stored in the database. While this may add complexity, it does give you fine grained
support to mix and match fire and forget messaging with messages that require durable persistence.
:::

One of the most important features in all of Jasper is the [persistent outbox](https://microservices.io/patterns/data/transactional-outbox.html) support and its easy integration into Marten.
If you're already familiar with the concept of an "outbox" (or "inbox"), skip to the sample code below.

Here's a common problem when using any kind of messaging strategy. Inside the handling for a single web request, you need to make some immediate writes to
the backing database for the application, then send a corresponding message out through your asynchronous messaging infrastructure. Easy enough, but here's a few ways
that could go wrong if you're not careful:

* The message is received and processed before the initial database writes are committed, and you get erroneous results because of that (I've seen this happen)
* The database transaction fails, but the message was still sent out, and you get inconsistency in the system
* The database transaction succeeds, but the message infrastructure fails some how, so you get inconsistency in the system

You could attempt to use some sort of [two phase commit](https://martinfowler.com/articles/patterns-of-distributed-systems/two-phase-commit.html)
between your database and the messaging infrastructure, but that has historically been problematic. This is where the "outbox" pattern comes into play to guarantee
that the outgoing message and database transaction both succeed or fail, and that the message is only sent out after the database transaction has succeeded.

Imagine a simple example where a Jasper handler is receiving a `CreateOrder` command that will span a brand new Marten `Order` document and also publish
an `OrderCreated` event through Jasper messaging. Using the outbox, that handler **in explicit, long hand form** is this:

<!-- snippet: sample_longhand_order_handler -->
<a id='snippet-sample_longhand_order_handler'></a>
```cs
public static async Task Handle(
    CreateOrder command,
    IDocumentSession session,
    IExecutionContext context,
    CancellationToken cancellation)
{
    // Connect the Marten session to the outbox
    // scoped to this specific command
    await context.EnlistInOutboxAsync(session);

    var order = new Order
    {
        Description = command.Description,
    };

    // Register the new document with Marten
    session.Store(order);

    // Hold on though, this message isn't actually sent
    // until the Marten session is committed
    await context.SendAsync(new OrderCreated(order.Id));

    // This makes the database commits, *then* flushed the
    // previously registered messages to Jasper's sending
    // agents
    await session.SaveChangesAsync(cancellation);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Order.cs#L109-L139' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_longhand_order_handler' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In the code above, the `OrderCreated` message is registered with the Jasper `IExecutionContext` for the current message, but nothing more than that is actually happening at that point.
When `IDocumentSession.SaveChangesAsync()` is called, Marten is persisting the new `Order` document **and** creating database records for the outgoing `OrderCreated` message
in the same transaction (and even in the same batched database command for maximum efficiency). After the database transaction succeeds, the pending messages are automatically sent to Jasper's
sending agents.

Now, let's play "what if:"

* What if the messaging broker is down? As long as the messages are persisted, Jasper will continue trying to send the persisted outgoing messages until the messaging broker is back up and available.
* What if the application magically dies after the database transaction but before the messages are sent through the messaging broker? Jasper will still be able to send these persisted messages from
  either another running application node or after the application is restarted.

The point here is that Jasper is doing store and forward mechanics with the outgoing messages and these messages will eventually be sent to the messaging infrastructure (unless they hit a designated expiration that you've defined).

In the section below on transactional middleware we'll see a shorthand way to simplify the code sample above and remove some repetitive ceremony.

## Outbox with ASP.Net Core

The Jasper outbox is also usable from within ASP.Net Core (really any code) controller or Minimal API handler code. Within an MVC controller, the `CreateOrder`
handling code would be:

<!-- snippet: sample_CreateOrderController -->
<a id='snippet-sample_createordercontroller'></a>
```cs
public class CreateOrderController : ControllerBase
{
    [HttpPost("/orders/create2")]
    public async Task Create(
        [FromBody] CreateOrder command,
        [FromServices] IDocumentSession session,
        [FromServices] IExecutionContext context)
    {
        // Gotta connection the Marten session into
        // the Jasper outbox
        await context.EnlistInOutboxAsync(session);

        var order = new Order
        {
            Description = command.Description,
        };

        // Register the new document with Marten
        session.Store(order);

        // Don't worry, this message doesn't go out until
        // after the Marten transaction succeeds
        await context.PublishAsync(new OrderCreated(order.Id));

        // Commit the Marten transaction
        await session.SaveChangesAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Order.cs#L21-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_createordercontroller' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

From a Minimal API, that could be this:

<!-- snippet: sample_create_order_through_minimal_api -->
<a id='snippet-sample_create_order_through_minimal_api'></a>
```cs
app.MapPost("/orders/create3", async (CreateOrder command, IDocumentSession session, IExecutionContext context) =>
{
    // Gotta connection the Marten session into
    // the Jasper outbox
    await context.EnlistInOutboxAsync(session);

    var order = new Order
    {
        Description = command.Description,
    };

    // Register the new document with Marten
    session.Store(order);

    // Don't worry, this message doesn't go out until
    // after the Marten transaction succeeds
    await context.PublishAsync(new OrderCreated(order.Id));

    // Commit the Marten transaction
    await session.SaveChangesAsync();
});
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Program.cs#L58-L82' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_create_order_through_minimal_api' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



## Transactional Middleware

::: tip
You will need to make the `IServiceCollection.AddMarten(...).IntegrateWithJasper()` call to add this middleware to a Jasper application.
:::

In the previous section we saw an example of incorporating Jasper's outbox with Marten transactions. We also wrote a fair amount of code to do so that could easily feel
repetitive over time. Using Jasper's transactional middleware support for Marten, the long hand handler above can become this equivalent:

<!-- snippet: sample_shorthand_order_handler -->
<a id='snippet-sample_shorthand_order_handler'></a>
```cs
// Note that we're able to avoid doing any kind of asynchronous
// code in this handler
[Transactional]
public static OrderCreated Handle(CreateOrder command, IDocumentSession session)
{
    var order = new Order
    {
        Description = command.Description
    };

    // Register the new document with Marten
    session.Store(order);

    // Utilizing Jasper's "cascading messages" functionality
    // to have this message sent through Jasper
    return new OrderCreated(order.Id);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Order.cs#L56-L76' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_shorthand_order_handler' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Or if you need to take more control over how the outgoing `OrderCreated` message is sent, you can use this slightly different alternative:

<!-- snippet: sample_shorthand_order_handler_alternative -->
<a id='snippet-sample_shorthand_order_handler_alternative'></a>
```cs
[Transactional]
public static ValueTask Handle(
    CreateOrder command,
    IDocumentSession session,
    IMessagePublisher publisher)
{
    var order = new Order
    {
        Description = command.Description
    };

    // Register the new document with Marten
    session.Store(order);

    // Utilizing Jasper's "cascading messages" functionality
    // to have this message sent through Jasper
    return publisher.SendAsync(
        new OrderCreated(order.Id),
        new DeliveryOptions{DeliverWithin = 5.Minutes()});
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Order.cs#L81-L104' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_shorthand_order_handler_alternative' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In both cases Jasper's transactional middleware for Marten is taking care of registering the Marten session with Jasper's outbox before you call into the message handler, and
also calling Marten's `IDocumentSession.SaveChangesAsync()` afterward. Used judiciously, this might allow you to avoid more messy or noising asynchronous code in your
application handler code.

::: tip
This [Transactional] attribute can appear on either the handler class that will apply to all the actions on that class, or on a specific action method.
:::

If so desired, you *can* also use a policy to apply the Marten transaction semantics with a policy. As an example, let's say that you want every message handler where the message type
name ends with "Command" to use the Marten transaction middleware. You could accomplish that
with a handler policy like this:

<!-- snippet: sample_CommandsAreTransactional -->
<a id='snippet-sample_commandsaretransactional'></a>
```cs
public class CommandsAreTransactional : IHandlerPolicy
{
    public void Apply(HandlerGraph graph, GenerationRules rules, IContainer container)
    {
        // Important! Create a brand new TransactionalFrame
        // for each chain
        graph
            .Chains
            .Where(x => x.MessageType.Name.EndsWith("Command"))
            .Each(x => x.Middleware.Add(new TransactionalFrame()));
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Jasper.Persistence.Testing/Marten/transactional_frame_end_to_end.cs#L87-L100' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_commandsaretransactional' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Then add the policy to your application like this:

<!-- snippet: sample_Using_CommandsAreTransactional -->
<a id='snippet-sample_using_commandsaretransactional'></a>
```cs
using var host = await Host.CreateDefaultBuilder()
    .UseJasper(opts =>
    {
        // And actually use the policy
        opts.Handlers.GlobalPolicy<CommandsAreTransactional>();
    }).StartAsync();
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Jasper.Persistence.Testing/Marten/transactional_frame_end_to_end.cs#L46-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_commandsaretransactional' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->



## Marten as Inbox

On the flip side of using Jasper's "outbox" support for outgoing messages, you can also choose to use the same message persistence for incoming messages such that
incoming messages are first persisted to the application's underlying Postgresql database before being processed. While
you *could* use this with external message brokers like Rabbit MQ, it's more likely this will be valuable for Jasper's [local queues](/guide/in-memory-bus).

Back to the sample Marten + Jasper integration from this page:

<!-- snippet: sample_integrating_jasper_with_marten -->
<a id='snippet-sample_integrating_jasper_with_marten'></a>
```cs
var builder = WebApplication.CreateBuilder(args);
builder.Host.ApplyOaktonExtensions();

builder.Services.AddMarten(opts =>
    {
        var connectionString = builder
            .Configuration
            .GetConnectionString("postgres");

        opts.Connection(connectionString);
        opts.DatabaseSchemaName = "orders";
    })
    // Optionally add Marten/Postgresql integration
    // with Jasper's outbox
    .IntegrateWithJasper();

    // You can also place the Jasper database objects
    // into a different database schema, in this case
    // named "jasper_messages"
    //.IntegrateWithJasper("jasper_messages");

builder.Host.UseJasper(opts =>
{
    // I've added persistent inbox
    // behavior to the "important"
    // local queue
    opts.LocalQueue("important")
        .UsePersistentInbox();
});
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Program.cs#L9-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_integrating_jasper_with_marten' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

But this time, focus on the Jasper configuration of the local queue named "important." By marking this local queue as persistent, any messages sent to this queue
in memory are first persisted to the underlying Postgresql database, and deleted when the message is successfully processed. This allows Jasper to grant a stronger
delivery guarantee to local messages and even allow messages to be processed if the current application node fails before the message is processed.

::: tip
There are some vague plans to add a little more efficient integration between Jasper and ASP.Net Core Minimal API, but we're not there yet.
:::

Or finally, it's less code to opt into Jasper's outbox by delegating to the [command bus](/guide/in-memory-bus) functionality as in this sample [Minimal API](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0) usage:

<!-- snippet: sample_delegate_to_command_bus_from_minimal_api -->
<a id='snippet-sample_delegate_to_command_bus_from_minimal_api'></a>
```cs
// Delegate directly to Jasper commands -- More efficient recipe coming later...
app.MapPost("/orders/create2", (CreateOrder command, ICommandBus bus)
    => bus.InvokeAsync(command));
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/WebApiWithMarten/Program.cs#L49-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_delegate_to_command_bus_from_minimal_api' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Saga Storage

Marten is an easy option for [persistent sagas](/guide/persistence/sagas) with Jasper. Yet again, to opt into using Marten as your saga storage mechanism in Jasper, you
just need to add the `IntegrateWithJasper()` option to your Marten configuration as shown in the [Getting Started](#getting-started) section above.

When using the Jasper + Marten integration, your stateful saga classes should be valid Marten document types that inherit from Jasper's `Saga` type, which generally means being a public class with a valid
Marten [identity member](https://martendb.io/documents/identity.html). Remember that your handler methods in Jasper can accept "method injected" dependencies from your underlying
IoC container.

TODO -- link to order saga sample

## Event Store & CQRS Support

TODO -- link to new OrderEventSourcingSample on GitHub

::: tip
This syntax or attribute might change before 2.0 is released. *If* anybody can ever come up with a better named alternative.
:::

That Jasper + Marten combination is optimized for efficient and productive development using a [CQRS architecture style](https://martinfowler.com/bliki/CQRS.html) with [Marten's event sourcing](https://martendb.io/events/) support.
Specifically, let's dive into the responsibilities of a typical command handler in a CQRS with event sourcing architecture:

1. Fetch any current state of the system that's necessary to evaluate or validate the incoming event
2. *Decide* what events should be emitted and captured in response to an incoming event
3. Manage concurrent access to system state
4. Safely commit the new events
5. Selectively publish some of the events based on system needs to other parts of your system or even external systems
6. Instrument all of the above

And then lastly, you're going to want some resiliency and selective retry capabilities for concurrent access violations or just normal infrastructure hiccups.

Let's just right into an example order management system. I'm going to model the order workflow with this aggregate model:

snippet: sample_Order_event_sourced_aggregate

TODO -- start here tomorrow.

At a minimum, we're going to want a command handler for this command message that marks an order item as ready to ship and then evaluates whether
or not based on the current state of the `Order` aggregate whether or not the logical order is ready to be shipped out:

snippet: sample_MarkItemReady

In the code above we're also utilizing Jasper's [outbox messaging](/guide/persistence/) support to both order and guarantee the delivery of a `ShipOrder` message when
the Marten transaction

Before getting into Jasper middleware strategies, let's first build out an MVC controller method for the command above:

snippet: sample_MarkItemController









