using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Runtime.Subscriptions.New;
using Jasper.Conneg;

namespace Jasper.Bus
{
    public class ServiceBusActivator
    {
        private readonly BusSettings _settings;
        private readonly IHandlerPipeline _pipeline;
        private readonly IDelayedJobProcessor _delayedJobs;
        private readonly ISubscriptionActivator _subscriptions;
        private readonly SerializationGraph _serialization;
        private readonly ITransport[] _transports;
        private readonly UriAliasLookup _lookups;

        public ServiceBusActivator(BusSettings settings, IHandlerPipeline pipeline, IDelayedJobProcessor delayedJobs, ISubscriptionActivator subscriptions, SerializationGraph serialization, ITransport[] transports, UriAliasLookup lookups)
        {
            _settings = settings;
            _pipeline = pipeline;
            _delayedJobs = delayedJobs;
            _subscriptions = subscriptions;
            _serialization = serialization;
            _transports = transports;
            _lookups = lookups;
        }

        public async Task Activate(HandlerGraph handlers, ChannelGraph channels, CapabilityGraph capabilities, JasperRuntime runtime)
        {
            var capabilityCompilation = capabilities.Compile(handlers, _serialization, channels, runtime, _transports);


            if (!_settings.DisableAllTransports)
            {
                await channels.ApplyLookups(_lookups);

                configureSerializationOrder(channels);

                channels.StartTransports(_pipeline, _transports);
                _delayedJobs.Start(_pipeline, channels);

                await _subscriptions.Activate();
            }

            runtime.Capabilities = await capabilityCompilation;
            if (runtime.Capabilities.Errors.Any() && _settings.ThrowOnValidationErrors)
            {
                throw new InvalidSubscriptionException(runtime.Capabilities.Errors);
            }
        }

        private void configureSerializationOrder(ChannelGraph channels)
        {
            var contentTypes = _serialization.Serializers
                .Select(x => x.ContentType).ToArray();

            var unknown = channels.AcceptedContentTypes.Where(x => !contentTypes.Contains(x)).ToArray();
            if (unknown.Any())
            {
                throw new UnknownContentTypeException(unknown, contentTypes);
            }

            foreach (var contentType in contentTypes)
            {
                channels.AcceptedContentTypes.Fill(contentType);
            }
        }
    }

    public class InvalidSubscriptionException : Exception
    {
        public InvalidSubscriptionException(string[] errors) : base($"Subscription errors detected:{Environment.NewLine}{errors.Select(e => $"* {e}{Environment.NewLine}")}")
        {
        }
    }

}
