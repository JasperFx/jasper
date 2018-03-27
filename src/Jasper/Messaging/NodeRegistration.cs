using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Http;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging
{
    /// <summary>
    /// Used by Jasper to register and unregister the current running node
    /// on startup and shutdown
    /// </summary>
    public class NodeRegistration : IHostedService
    {
        private readonly MessagingSettings _settings;
        private readonly INodeDiscovery _nodes;
        private readonly JasperRuntime _runtime;
        private readonly IMessageLogger _logger;
        private readonly HttpTransportSettings _httpSettings;

        public NodeRegistration(MessagingSettings settings, INodeDiscovery nodes, JasperRuntime runtime,
            IMessageLogger logger, HttpTransportSettings httpSettings)
        {
            _settings = settings;
            _nodes = nodes;
            _runtime = runtime;
            _logger = logger;
            _httpSettings = httpSettings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var local = new ServiceNode(_settings)
                {
                    MessagesUrl = _httpSettings.RelativeUrl,
                    HttpEndpoints = _runtime.HttpAddresses?.Select(x => x.ToUri().ToMachineUri()).Distinct()
                        .ToArray()
                };

                _runtime.Node = local;

                await _nodes.Register(local);
            }
            catch (Exception e)
            {
                _logger
                    .LogException(e, message: "Failure when trying to register the node with " + _nodes);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _nodes.UnregisterLocalNode();
        }
    }
}
