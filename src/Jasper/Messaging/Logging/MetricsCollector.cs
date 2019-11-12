using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.WorkerQueues;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging.Logging
{
    public class MetricsCollector : BackgroundService
    {
        private readonly IMessageLogger _logger;
        private readonly IMetrics _metrics;
        private readonly IEnvelopePersistence _persistence;
        private readonly JasperOptions _options;
        private readonly IWorkerQueue _workers;

        public MetricsCollector(IMetrics metrics, IEnvelopePersistence persistence, IMessageLogger logger,
            JasperOptions options, IWorkerQueue workers)
        {
            _metrics = metrics;
            _persistence = persistence;
            _logger = logger;
            _options = options;
            _workers = workers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_options.Advanced.MetricsCollectionSamplingInterval, stoppingToken);

                _metrics.LogLocalWorkerQueueDepth(_workers.QueuedCount);

                try
                {
                    var counts = await _persistence.Admin.GetPersistedCounts();
                    _metrics.LogPersistedCounts(counts);
                }
                catch (Exception e)
                {
                    _logger.LogException(e);
                }
            }
        }
    }
}
