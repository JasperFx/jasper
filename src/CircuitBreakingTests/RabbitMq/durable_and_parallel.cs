using Baseline.Dates;
using IntegrationTests;
using Jasper;
using Jasper.ErrorHandling;
using Jasper.Persistence.Marten;
using Jasper.RabbitMQ;
using Marten;
using Xunit.Abstractions;

namespace CircuitBreakingTests.RabbitMq;

public class durable_and_parallel : CircuitBreakerIntegrationContext
{
    public durable_and_parallel(ITestOutputHelper output) : base(output)
    {
    }

    protected override void configureListener(JasperOptions opts)
    {
        opts.Services.AddMarten(m =>
        {
            m.Connection(Servers.PostgresConnectionString);
            m.DatabaseSchemaName = "circuit_breaker";
        }).IntegrateWithJasper();

        // Requeue failed messages.
        opts.Handlers.OnException<BadImageFormatException>().Or<DivideByZeroException>()
            .Requeue();

        opts.PublishAllMessages().ToRabbitQueue("circuit4");
        opts.ListenToRabbitQueue("circuit4").CircuitBreaker(cb =>
        {
            cb.MinimumThreshold = 250;
            cb.PauseTime = 10.Seconds();
            cb.TrackingPeriod = 1.Minutes();
            cb.FailurePercentageThreshold = 20;
        }).UseInbox().MaximumParallelMessages(5);
    }
}