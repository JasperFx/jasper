using IntegrationTests;
using Jasper;
using Jasper.Persistence.Marten;
using Jasper.RabbitMQ;
using Marten;
using Oakton;
using Oakton.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ApplyOaktonExtensions();

builder.Host.UseJasper(opts =>
{
    opts.PublishAllMessages()
        .ToRabbitQueue("issue_events");

    opts.UseRabbitMq()
        .AutoProvision()
        .AutoPurgeOnStartup();

});

builder.Services.AddResourceSetupOnStartup();

builder.Services.AddMarten(opts =>
{
    // I think you would most likely pull the connection string from
    // configuration like this:
    // var martenConnectionString = builder.Configuration.GetConnectionString("marten");
    // opts.Connection(martenConnectionString);

    opts.Connection(Servers.PostgresConnectionString);
    opts.DatabaseSchemaName = "issues";
}).IntegrateWithJasper("issue_service");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.RunOaktonCommandsSynchronously(args);
