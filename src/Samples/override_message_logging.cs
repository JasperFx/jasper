using System.Threading.Tasks;
using Jasper;
using Jasper.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Samples
{
    public static class override_message_logging
    {
        public static async Task AppWithCustomLogging()
        {
            #region sample_AppWithCustomLogging

            using var host = Host.CreateDefaultBuilder()
                .UseJasper(opts => { opts.Services.AddSingleton<IMessageLogger, CustomMessageLogger>(); }).StartAsync();

            #endregion
        }
    }


    #region sample_CustomMessageLogger

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

    #endregion
}
