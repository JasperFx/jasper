using Jasper;
using Jasper.Persistence.Marten;
using Marten;
using Oakton;
using Oakton.Resources;
using OrderSagaSample;

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


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Do all necessary database setup on startup
builder.Services.AddResourceSetupOnStartup();

// The defaults are good enough here
builder.Host.UseJasper();

var app = builder.Build();

// Just delegating to Jasper's local command bus for all
app.MapPost("/start", (StartOrder start, ICommandBus bus) => bus.InvokeAsync(start));
app.MapPost("/complete", (StartOrder start, ICommandBus bus) => bus.InvokeAsync(start));
app.MapGet("/all", (IQuerySession session) => session.Query<Order>().ToListAsync());

app.UseSwagger();
app.UseSwaggerUI();

return await app.RunOaktonCommands(args);




