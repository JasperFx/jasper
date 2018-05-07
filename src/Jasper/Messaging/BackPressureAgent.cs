using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging
{
    public class BackPressureAgent : BackgroundService
    {
        private readonly IMessagingRoot _root;

        public BackPressureAgent(IMessagingRoot root)
        {
            _root = root;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_root.Settings.BackPressurePollingInterval, stoppingToken);
                try
                {
                    ApplyBackPressure();
                }
                catch (Exception e)
                {
                    _root.Logger.LogException(e);
                }
            }
        }


        public void ApplyBackPressure()
        {
            double ratio = (double)_root.Workers.QueuedCount / (double)_root.Settings.MaximumLocalEnqueuedBackPressureThreshold;

            if (_root.ListeningStatus == ListeningStatus.Accepting && ratio > 1.0)
            {
                _root.ListeningStatus = ListeningStatus.TooBusy;
            }
            else if (_root.ListeningStatus == ListeningStatus.TooBusy && ratio < 0.8)
            {
                _root.ListeningStatus = ListeningStatus.Accepting;
            }
        }
    }
}
