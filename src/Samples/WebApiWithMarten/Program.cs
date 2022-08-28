using Jasper;
using Jasper.Persistence.Marten;
using Marten;
using Marten.AspNetCore;
using Oakton;
using Spectre.Console.Cli;
using WebApiWithMarten;

#region sample_integrating_jasper_with_marten

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

#endregion

var app = builder.Build();


app.MapGet("/orders", (IQuerySession session, HttpContext context)
    => session.Query<Order>().WriteArray(context));

#region sample_delegate_to_command_bus_from_minimal_api

// Delegate directly to Jasper commands -- More efficient recipe coming later...
app.MapPost("/orders/create2", (CreateOrder command, ICommandBus bus)
    => bus.InvokeAsync(command));

#endregion


#region sample_create_order_through_minimal_api

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

#endregion

// Lot of Jasper and Marten diagnostics and administrative tools
// come through Oakton command line support
return await app.RunOaktonCommands(args);
