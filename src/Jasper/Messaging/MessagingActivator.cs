using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Transports;
using Lamar.Util;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging
{
    [CacheResolver]
    public class MessagingActivator : IHostedService
    {
        private readonly JasperRuntime _runtime;
        private readonly IMessagingRoot _root;

        public MessagingActivator(JasperRuntime runtime, IMessagingRoot root)
        {
            _runtime = runtime;
            _root = root;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var messaging = _runtime.Registry.Messaging;
            return _root.Activate(messaging.LocalWorker, _runtime,
                _runtime.Registry.CodeGeneration, new PerfTimer());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _runtime.Settings.StopAll();

            // Nothing right now
            return Task.CompletedTask;
        }
    }
}
