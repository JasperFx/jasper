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
using Jasper.Codegen;
using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using Policies = Jasper.Bus.Configuration.Policies;

namespace Jasper.Bus
{
    public class ServiceBusFeature : IFeature
    {
        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();

        public GenerationConfig Generation { get; } = new GenerationConfig("JasperBus.Generated");

        public ChannelGraph Channels { get; } = new ChannelGraph();

        public Policies Policies { get; } = new Policies();
        public bool DelayedJobsRunInMemory { get; set; } = true;

        public readonly ServiceRegistry Services = new ServiceBusRegistry();

        public void Dispose()
        {
            Channels.Dispose();
        }

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
                var container = runtime.Container;

                // TODO -- will need to be smart enough to do the conglomerate
                // generation config of the base, with service bus specific stuff
                _graph.Compile(generation, container);

                var transports = container.GetAllInstances<ITransport>().ToArray();

                Channels.UseTransports(transports);

                configureSerializationOrder(runtime);

                var pipeline = container.GetInstance<IHandlerPipeline>();

                verifyTransports(transports);

                if (Channels.ControlChannel != null)
                {
                    Channels.ControlChannel.MaximumParallelization = 1;
                }


                startTransports(transports, pipeline);

                container.GetInstance<IDelayedJobProcessor>().Start(pipeline, Channels);

                container.GetInstance<ISubscriptionActivator>().Activate();
            });
        }

        private void startTransports(ITransport[] transports, IHandlerPipeline pipeline)
        {
            foreach (var transport in transports)
            {
                transport.Start(pipeline, Channels);

                Channels
                    .Where(x => x.Uri.Scheme == transport.Protocol && x.Sender == null)
                    .Each(x => { x.Sender = new NulloSender(transport, x.Uri); });
            }
        }

        private void verifyTransports(ITransport[] transports)
        {
            var unknowns = Channels.Where(x => transports.All(t => t.Protocol != x.Uri.Scheme)).ToArray();
            if (unknowns.Length > 0)
            {
                throw new UnknownTransportException(unknowns);
            }
        }

        private void configureSerializationOrder(JasperRuntime runtime)
        {
            var contentTypes = runtime.Container.GetAllInstances<IMessageSerializer>()
                .Select(x => x.ContentType).ToArray();

            var unknown = Channels.AcceptedContentTypes.Where(x => !contentTypes.Contains(x)).ToArray();
            if (unknown.Any())
            {
                throw new UnknownContentTypeException(unknown, contentTypes);
            }

            foreach (var contentType in contentTypes)
            {
                Channels.AcceptedContentTypes.Fill(contentType);
            }
        }

        private async Task<Registry> bootstrap(JasperRegistry registry)
        {
            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            _graph = new HandlerGraph();
            _graph.AddRange(calls);
            _graph.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionRequested())));
            _graph.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionsChanged())));

            _graph.Group();
            Policies.Apply(_graph);

            Services.For<HandlerGraph>().Use(_graph);
            Services.For<ChannelGraph>().Use(Channels);

            if (registry.Logging.UseConsoleLogging)
            {
                Services.For<IBusLogger>().Add<ConsoleBusLogger>();
            }

            if (DelayedJobsRunInMemory)
            {
                Channels.AddChannelIfMissing(InMemoryDelayedJobProcessor.Queue).Incoming = true;

                Services.ForSingletonOf<IDelayedJobProcessor>().Use<InMemoryDelayedJobProcessor>();
            }

            return Services;
        }
    }

    public class UnknownTransportException : Exception
    {
        public UnknownTransportException(ChannelNode[] nodes) : base("Unknown transport types for " + nodes.Select(x => x.Uri.ToString()).Join(", "))
        {
        }
    }
}
