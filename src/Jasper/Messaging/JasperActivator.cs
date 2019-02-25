using System;
using System.Threading;
using System.Threading.Tasks;
using Lamar;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging
{
    /// <summary>
    /// Used to start up all Jasper messaging listeners and senders
    /// </summary>
    public class JasperActivator : IHostedService
    {
        private readonly JasperRegistry _registry;
        private readonly IMessagingRoot _root;
        private readonly IContainer _container;

        public JasperActivator(JasperRegistry registry, IMessagingRoot root, IContainer container)
        {
            _registry = registry;
            _root = root;
            _container = container;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _registry.Messaging.Compiling;

                _root.Activate(_registry.Messaging.LocalWorker, _registry.CodeGeneration, _container);
            }
            catch (Exception e)
            {
                _root.Logger.LogException(e);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
