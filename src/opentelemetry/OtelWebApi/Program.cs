using Jasper;
using Jasper.RabbitMQ;
using Jasper.Transports.Tcp;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtelMessages;
using OtelWebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseJasper(opts =>
{
    opts.ServiceName = "WebApi";
    opts.ApplicationAssembly = typeof(InitialCommandHandler).Assembly;

    opts.PublishMessage<TcpMessage1>().ToPort(MessagingConstants.Subscriber1Port);

    opts.ListenAtPort(MessagingConstants.WebApiPort);

    opts.UseRabbitMq()
        .DeclareQueue(MessagingConstants.Subscriber1Queue)
        .DeclareQueue(MessagingConstants.Subscriber2Queue)
        .DeclareExchange(MessagingConstants.OtelExchangeName, ex =>
        {
            ex.BindQueue(MessagingConstants.Subscriber1Queue);
            ex.BindQueue(MessagingConstants.Subscriber2Queue);
        })
        .AutoProvision().AutoPurgeOnStartup();

    opts.PublishMessage<RabbitMessage1>()
        .ToRabbitExchange(MessagingConstants.OtelExchangeName);

});

builder.Services.AddControllers();
var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("OtelWebApi")
    .AddAttributes(new Dictionary<string, object> {
        { "environment", "production" }
    })
    .AddTelemetrySdk();

builder.Services.AddOpenTelemetryTracing(x =>
{
    x

        .AddJaegerExporter()
        .AddAspNetCoreInstrumentation()
        .AddJasper();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Doing this just to get JSON formatters in here
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/", c => c.Response.WriteAsync("Hello."));

app.Run();
