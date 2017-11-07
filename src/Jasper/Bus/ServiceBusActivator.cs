using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Jasper.Conneg;
using Microsoft.CodeAnalysis;

namespace Jasper.Bus
{
    public class ServiceBusActivator
    {
        private readonly BusSettings _settings;
        private readonly IHandlerPipeline _pipeline;
        private readonly IDelayedJobProcessor _delayedJobs;
        private readonly SerializationGraph _serialization;
        private readonly ITransport[] _transports;
        private readonly UriAliasLookup _lookups;
        private readonly INodeDiscovery _nodes;
        private readonly IWorkerQueue _workerQueue;
        private readonly CompositeLogger _logger;

        public ServiceBusActivator(BusSettings settings, IHandlerPipeline pipeline, IDelayedJobProcessor delayedJobs, BusMessageSerializationGraph serialization, IEnumerable<ITransport> transports, UriAliasLookup lookups, INodeDiscovery nodes, IWorkerQueue workerQueue, CompositeLogger logger)
        {
            _settings = settings;
            _pipeline = pipeline;
            _delayedJobs = delayedJobs;
            _serialization = serialization;
            _transports = transports.ToArray();
            _lookups = lookups;
            _nodes = nodes;
            _workerQueue = workerQueue;
            _logger = logger;
        }

        public async Task Activate(HandlerGraph handlers, CapabilityGraph capabilities, JasperRuntime runtime, ChannelGraph channels)
        {
            var capabilityCompilation = capabilities.Compile(handlers, _serialization, channels, runtime, _transports, _lookups);

            var transports = _transports.Where(x => _settings.StateFor(x.Protocol) != TransportState.Disabled)
                .ToArray();

            _settings.Workers.Compile(handlers.Chains.Select(x => x.MessageType));


            if (!_settings.DisableAllTransports)
            {
                await _settings.ApplyLookups(_lookups);

                channels.Start(_settings, transports, _lookups);

                _delayedJobs.Start(_workerQueue);



                try
                {
                    var local = new TransportNode(_settings);

                    await _nodes.Register(local);
                }
                catch (Exception e)
                {
                    _logger.LogException(e, message:"Failure when trying to register the node with " + _nodes);
                }

            }

            runtime.Capabilities = await capabilityCompilation;
            if (runtime.Capabilities.Errors.Any() && _settings.ThrowOnValidationErrors)
            {
                throw new InvalidSubscriptionException(runtime.Capabilities.Errors);
            }
        }

    }
}
