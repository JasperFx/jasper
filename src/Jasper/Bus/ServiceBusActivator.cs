﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;

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

        public ServiceBusActivator(BusSettings settings, IHandlerPipeline pipeline, IDelayedJobProcessor delayedJobs, BusMessageSerializationGraph serialization, IEnumerable<ITransport> transports, UriAliasLookup lookups, INodeDiscovery nodes)
        {
            _settings = settings;
            _pipeline = pipeline;
            _delayedJobs = delayedJobs;
            _serialization = serialization;
            _transports = transports.ToArray();
            _lookups = lookups;
            _nodes = nodes;
        }

        public async Task Activate(HandlerGraph handlers, CapabilityGraph capabilities, JasperRuntime runtime, OutgoingChannels channels)
        {
            var capabilityCompilation = capabilities.Compile(handlers, _serialization, channels, runtime, _transports, _lookups);

            var transports = _transports.Where(x => x.State == TransportState.Enabled).ToArray();

            if (!_settings.DisableAllTransports)
            {
                await _settings.ApplyLookups(_lookups);



                channels.StartTransports(_pipeline, _settings, transports);

                _delayedJobs.Start(_pipeline, channels);

                var local = new TransportNode(channels, _settings.MachineName);

                await _nodes.Register(local);

            }

            runtime.Capabilities = await capabilityCompilation;
            if (runtime.Capabilities.Errors.Any() && _settings.ThrowOnValidationErrors)
            {
                throw new InvalidSubscriptionException(runtime.Capabilities.Errors);
            }
        }

    }
}
