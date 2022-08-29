using Jasper;
using Jasper.Persistence.Marten;
using Marten;
using Oakton;
using OrderEventSourcingSample;

var builder = WebApplication.CreateBuilder(args);

// Not 100% necessary, but enables some extra command line diagnostics
builder.Host.ApplyOaktonExtensions();

// Adding Marten
builder.Services.AddMarten(opts =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Marten");
        opts.Connection(connectionString);
        opts.DatabaseSchemaName = "orders";
    })

    // Adding the Jasper integration for Marten.
    .IntegrateWithJasper();

builder.Host.UseJasper();

var app = builder.Build();

app.MapPost("/items/ready", (MarkItemReady command, ICommandBus bus) => bus.InvokeAsync(command));

return await app.RunOaktonCommands(args);
