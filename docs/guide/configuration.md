# Configuration

::: warning
Jasper requires the usage of the [Lamar](https://jasperfx.github.io/lamar) IoC container, and the call
to `UseJasper()` quietly replaces the built in .NET container with Lamar.

Lamar was originally written specifically to support Jasper's runtime model as well as to be a higher performance
replacement for the older StructureMap tool.
:::

Jasper is configured with the `IHostBuilder.UseJasper()` extension methods, with the actual configuration
living on a single `JasperOptions` object.

## With ASP.NET Core

Below is a sample of adding Jasper to an ASP.NET Core application that is bootstrapped with
`WebApplicationBuilder`:

<!-- snippet: sample_Quickstart_Program -->
<a id='snippet-sample_quickstart_program'></a>
```cs
using Jasper;
using Quickstart;

var builder = WebApplication.CreateBuilder(args);

// For now, this is enough to integrate Jasper into
// your application, but there'll be *much* more
// options later of course :-)
builder.Host.UseJasper();

// Some in memory services for our application, the
// only thing that matters for now is that these are
// systems built by the application's IoC container
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<IssueRepository>();

var app = builder.Build();

// An endpoint to create a new issue
app.MapPost("/issues/create", (CreateIssue body, ICommandBus bus) => bus.InvokeAsync(body));

// An endpoint to assign an issue to an existing user
app.MapPost("/issues/assign", (AssignIssue body, ICommandBus bus) => bus.InvokeAsync(body));

app.Run();
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Samples/Quickstart/Program.cs#L1-L30' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_quickstart_program' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## "Headless" Applications

:::tip
The `JasperOptions.Services` property can be used to add additional IoC service registrations with
either the standard .NET `IServiceCollection` model or the [Lamar ServiceRegistry](https://jasperfx.github.io/lamar/guide/ioc/registration/registry-dsl.html) syntax.
:::

For "headless" console applications with no user interface or HTTP service endpoints, the bootstrapping
can be done with just the `HostBuilder` mechanism as shown below:

<!-- snippet: sample_bootstrapping_headless_service -->
<a id='snippet-sample_bootstrapping_headless_service'></a>
```cs
return await Host.CreateDefaultBuilder(args)
    .UseJasper(opts =>
    {
        opts.ServiceName = "Subscriber1";

        opts.Handlers.Discovery(source =>
        {
            source.DisableConventionalDiscovery();
            source.IncludeType<Subscriber1Handlers>();
        });

        opts.ListenAtPort(MessagingConstants.Subscriber1Port);

        opts.UseRabbitMq().AutoProvision();

        opts.ListenToRabbitQueue(MessagingConstants.Subscriber1Queue);

        // Publish to the other subscriber
        opts.PublishMessage<RabbitMessage2>().ToRabbitQueue(MessagingConstants.Subscriber2Queue);

        // Add Open Telemetry tracing
        opts.Services.AddOpenTelemetryTracing(builder =>
        {
            builder
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService("Subscriber1"))
                .AddJaegerExporter()

                // Add Jasper as a source
                .AddSource("Jasper");
        });
    })
    .RunOaktonCommands(args);
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/opentelemetry/Subscriber1/Program.cs#L10-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bootstrapping_headless_service' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
