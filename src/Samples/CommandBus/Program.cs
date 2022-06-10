using CommandBusSamples;
using Jasper;
using Marten;
using Oakton;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(opts =>
{
    opts.Connection("some connection string to Postgresql");
});

// Adding Jasper as a straight up Command Bus
builder.Host.UseJasper();

var app = builder.Build();

app.MapPost("/reservations/confirm", (ConfirmReservation command, ICommandBus bus) => bus.EnqueueAsync(command));

return await app.RunOaktonCommands(args);
