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
        private readonly IEnvelopePersistor _persistor;
        private readonly JasperOptions _settings;
        private readonly IWorkerQueue _workers;

        public MetricsCollector(IMetrics metrics, IEnvelopePersistor persistor, IMessageLogger logger,
            JasperOptions settings, IWorkerQueue workers)
        {
            _metrics = metrics;
            _persistor = persistor;
            _logger = logger;
            _settings = settings;
            _workers = workers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_settings.MetricsCollectionSamplingInterval, stoppingToken);

                _metrics.LogLocalWorkerQueueDepth(_workers.QueuedCount);

                try
                {
                    var counts = await _persistor.GetPersistedCounts();
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
