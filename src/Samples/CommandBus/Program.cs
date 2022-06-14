using Baseline.Dates;
using CommandBusSamples;
using Jasper;
using Jasper.ErrorHandling;
using Jasper.Persistence.Marten;
using Marten;
using Npgsql;
using Oakton;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(opts =>
{
    opts.Connection("Host=localhost;Port=5433;Database=postgres;Username=postgres;password=postgres");
})
    // NEW! Adding Jasper outbox integration to Marten in the "messages"
    // database schema
    .IntegrateWithJasper("messages");

// Adding Jasper as a straight up Command Bus
builder.Host.UseJasper(opts =>
{
    // Just setting up some retries on transient database connectivity errors
    opts.Handlers.OnException<NpgsqlException>().OrInner<NpgsqlException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

    // NEW! Apply the durable inbox/outbox functionality to the two in-memory queues
    opts.DefaultLocalQueue.DurablyPersistedLocally();
    opts.LocalQueue("Notifications").DurablyPersistedLocally();

    // And I just opened a GitHub issue to make this config easier...
});

var app = builder.Build();

// This isn't *quite* the most efficient way to do this,
// but it's simple to understand, so please just let it go...
app.MapPost("/reservations/confirm", (ConfirmReservation command, ICommandBus bus) => bus.EnqueueAsync(command));

// This opts into using Oakton for extended command line options for this app
// Oakton is also a transitive dependency of Jasper itself
return await app.RunOaktonCommands(args);
