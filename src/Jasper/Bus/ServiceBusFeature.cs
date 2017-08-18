using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Runtime.Subscriptions.New;
using Jasper.Bus.Transports.InMemory;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Conneg;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using CapabilityGraph = Jasper.Bus.Runtime.Subscriptions.New.CapabilityGraph;
using Policies = Jasper.Bus.Configuration.Policies;

namespace Jasper.Bus
{
    public class ServiceBusFeature : IFeature
    {
        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();

        public CapabilityGraph Capabilities = new CapabilityGraph();

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

        async Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            var container = runtime.Container;


            _graph.Compile(generation, container);

            // TODO -- create a new "BusStartup" class that does all of this kind of stuff
            var lookups = container.GetAllInstances<IUriLookup>();
            await Channels.ApplyLookups(lookups);



            configureSerializationOrder(runtime);

            var pipeline = container.GetInstance<IHandlerPipeline>();

            var transports = container.GetAllInstances<ITransport>().ToArray();

            Channels.StartTransports(pipeline, transports);

            container.GetInstance<IDelayedJobProcessor>().Start(pipeline, Channels);

            await container.GetInstance<ISubscriptionActivator>().Activate();
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            var incoming = Channels.Where(x => x.Incoming).Distinct().ToArray();
            if (incoming.Any())
            {
                foreach (var node in incoming)
                {
                    writer.WriteLine($"Listening for messages at {node.Uri}");
                }
            }
            else
            {
                writer.WriteLine("No incoming message channels configured");
            }
        }

        private void configureSerializationOrder(JasperRuntime runtime)
        {
            var contentTypes = runtime.Container.GetAllInstances<ISerializer>()
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

            if (registry.ApplicationAssembly != null)
            {
                Services.Scan(_ =>
                {
                    _.Assembly(registry.ApplicationAssembly);
                    _.AddAllTypesOf<IMediaReader>();
                    _.AddAllTypesOf<IMediaWriter>();
                });
            }

            if (registry.Logging.UseConsoleLogging)
            {
                Services.For<IBusLogger>().Add<ConsoleBusLogger>();
            }

            if (DelayedJobsRunInMemory)
            {
                Channels.AddChannelIfMissing(LoopbackTransport.Delayed).Incoming = true;

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
