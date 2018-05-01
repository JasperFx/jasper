using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging
{
    public class BackPressureAgent : IHostedService
    {
        private readonly IMessagingRoot _root;
        private Timer _timer;

        public BackPressureAgent(IMessagingRoot root)
        {
            _root = root;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(s => ApplyBackPressure(), null, _root.Settings.BackPressurePollingInterval, _root.Settings.BackPressurePollingInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        public void ApplyBackPressure()
        {
            double ratio = (double)_root.Workers.QueuedCount / (double)_root.Settings.MaximumLocalEnqueuedBackPressureThreshold;

            if (_root.ListeningStatus == ListeningStatus.Accepting && ratio > 1.0)
            {
                _root.ListeningStatus = ListeningStatus.TooBusy;
            }
            else if (_root.ListeningStatus == ListeningStatus.TooBusy && ratio < 0.9)
            {
                _root.ListeningStatus = ListeningStatus.Accepting;
            }
        }
    }
}
