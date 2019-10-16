using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jasper.Testing.Samples
{
    public class override_message_logging
    {
    }

    // SAMPLE: AppWithCustomLogging
    public class AppWithCustomLogging : JasperRegistry
    {
        public AppWithCustomLogging()
        {
            Services.AddSingleton<IMessageLogger, CustomMessageLogger>();
        }
    }
    // ENDSAMPLE

    // SAMPLE: CustomMessageLogger
    public class CustomMessageLogger : MessageLogger
    {
        private readonly ILogger<CustomMessageLogger> _logger;

        public CustomMessageLogger(ILoggerFactory factory, IMetrics metrics, ILogger<CustomMessageLogger> logger) :
            base(factory, metrics)
        {
            _logger = logger;
        }

        public override void ExecutionStarted(Envelope envelope)
        {
            base.ExecutionStarted(envelope);
            _logger.LogInformation(
                $"Executing envelope {envelope.Id}, caused by {envelope.CausationId}, correlated by {envelope.CorrelationId}");
        }

        // And any other events you might care about
    }

    // ENDSAMPLE
}
