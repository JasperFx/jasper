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
        private readonly JasperOptions _options;
        private readonly IMessagingRoot _root;
        private readonly IContainer _container;

        public JasperActivator(JasperOptions options, IMessagingRoot root, IContainer container)
        {
            _options = options;
            _root = root;
            _container = container;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _options.HandlerGraph.Compiling;

                await _root.Activate(_options.CodeGeneration, _container);
            }
            catch (Exception e)
            {
                _root.Logger.LogException(e);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _root.StopAsync(cancellationToken);
        }
    }
}
