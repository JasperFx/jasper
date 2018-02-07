using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Util;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Scheduled;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Jasper.Conneg;
using Jasper.Http.Transport;
using Microsoft.Extensions.ObjectPool;

namespace Jasper.Bus
{
    public class MessagingRoot : IDisposable
    {
        private readonly BusSettings _settings;
        private readonly HandlerGraph _handlers;
        private readonly IReplyWatcher _replies;
        private readonly IPersistence _persistence;
        private readonly IChannelGraph _channels;
        private readonly ITransport[] _transports;

        public MessagingRoot(
            ObjectPoolProvider pooling,
            BusSettings settings,
            HandlerGraph handlers,
            Forwarders forwarders,
            IPersistence persistence,
            IChannelGraph channels,
            ISubscriptionsRepository subscriptions,
            IEnumerable<ISerializerFactory> serializers,
            IEnumerable<IMessageDeserializer> readers,
            IEnumerable<IMessageSerializer> writers,
            IMessageLogger[] loggers,
            ITransport[] transports,
            IEnumerable<IMissingHandler> missingHandlers,
            IEnumerable<IUriLookup> lookups)
        {
            _settings = settings;
            _handlers = handlers;
            _replies = new ReplyWatcher();
            _persistence = persistence;
            _channels = channels;
            _transports = transports;



            Lookup = new UriAliasLookup(lookups);


            Serialization = new BusMessageSerializationGraph(pooling, settings, handlers, forwarders, serializers,
                readers, writers);

            Logger = new CompositeMessageLogger(loggers);

            Pipeline = new HandlerPipeline(Serialization, handlers, _replies, Logger, missingHandlers,
                new Lazy<IServiceBus>(Build));

            Workers = new WorkerQueue(Logger, Pipeline, settings);

            Router = new MessageRouter(Serialization, channels, subscriptions, handlers, Logger, Lookup, settings);

            ScheduledJobs = new InMemoryScheduledJobProcessor();
        }

        public IScheduledJobProcessor ScheduledJobs { get; }

        public IMessageRouter Router { get; }

        public UriAliasLookup Lookup { get; }

        public WorkerQueue Workers { get; }

        public HandlerPipeline Pipeline { get; }

        public CompositeMessageLogger Logger { get; }

        public BusMessageSerializationGraph Serialization { get; }

        public IServiceBus Build()
        {
            return new ServiceBus(Router, _replies, Pipeline, Serialization, _settings, _channels, _persistence, Logger);
        }

        public async Task Activate(LocalWorkerSender localWorker, CapabilityGraph capabilities, JasperRuntime runtime,
            GenerationRules generation, PerfTimer timer)
        {
            timer.MarkStart("ServiceBusActivator");

            _handlers.Compile(generation, runtime, timer);


            var capabilityCompilation = capabilities.Compile(_handlers, Serialization, _channels, runtime, _transports, Lookup);





            var transports = _transports.Where(x => _settings.StateFor(x.Protocol) != TransportState.Disabled)
                .ToArray();

            timer.Record("WorkersGraph.Compile", () =>
            {
                _settings.Workers.Compile(_handlers.Chains.Select(x => x.MessageType));
            });



            localWorker.Start(_persistence, Workers);

            if (!_settings.DisableAllTransports)
            {
                timer.MarkStart("ApplyLookups");

                await _settings.ApplyLookups(Lookup);

                timer.MarkFinished("ApplyLookups");


                timer.Record("ChannelGraph.Start",
                    () => { _channels.As<ChannelGraph>().Start(_settings, transports, Lookup, capabilities, Logger, Workers); });

                ScheduledJobs.Start(Workers);
            }

            runtime.Capabilities = await capabilityCompilation;
            if (runtime.Capabilities.Errors.Any() && _settings.ThrowOnValidationErrors)
            {
                throw new InvalidSubscriptionException(runtime.Capabilities.Errors);
            }

            timer.MarkFinished("ServiceBusActivator");
        }

        public void Dispose()
        {
            ScheduledJobs.Dispose();


        }
    }
}
