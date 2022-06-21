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

    opts.UseRabbitMq(factory =>
        {
            // Just connecting with defaults, but showing
            // how you *could* customize the connection to Rabbit MQ
            factory.HostName = "localhost";
            factory.Port = 5672;

            // Other possibilities
            // factory.UserName = builder.Configuration["rabbit_user"];
            // factory.Password = builder.Configuration["rabbit_password"];

        })
        // Let Jasper try to build all known Rabbit MQ objects as configured
        .AutoProvision()

        // Think this is mostly just good for testing,
        // but purge all known queues at start up time
        .AutoPurgeOnStartup();

    // Or, if you prefer
    var rabbitUri = new Uri(builder.Configuration["rabbit_uri"]);
    opts.UseRabbitMq(rabbitUri)
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

    // I'm putting the inbox/outbox tables into a separate "issue_service" schema
}).IntegrateWithJasper("issue_service");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

await app.RunOaktonCommands(args);
