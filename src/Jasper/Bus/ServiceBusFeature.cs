using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Conneg;
using StructureMap;
using CapabilityGraph = Jasper.Bus.Runtime.Subscriptions.CapabilityGraph;

namespace Jasper.Bus
{
    public class ServiceBusFeature : IFeature
    {
        private readonly OutgoingChannels _channels = new OutgoingChannels();
        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();


        public CapabilityGraph Capabilities = new CapabilityGraph();

        public GenerationConfig Generation { get; } = new GenerationConfig("JasperBus.Generated");

        public BusSettings Settings { get; } = new BusSettings();

        public bool DelayedJobsRunInMemory { get; set; } = true;

        public readonly ServiceRegistry Services = new ServiceBusRegistry();

        public void Dispose()
        {
        }

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            var container = runtime.Container;


            _graph.Compile(generation, container);

            return runtime.Get<ServiceBusActivator>().Activate(_graph, Capabilities, runtime, _channels);
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            var transports = runtime.Get<ITransport[]>();
            foreach (var transport in transports.Where(x => x.State == TransportState.Enabled))
            {
                transport.Describe(writer);
            }


//            var incoming = Channels.Where(x => x.Incoming).Distinct().ToArray();
//            if (incoming.Any())
//            {
//                foreach (var node in incoming)
//                {
//                    writer.WriteLine($"Listening for messages at {node.Uri}");
//                }
//            }
//            else
//            {
//                writer.WriteLine("No incoming message channels configured");
//            }
        }



        private async Task<Registry> bootstrap(JasperRegistry registry)
        {
            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            _graph = new HandlerGraph();
            _graph.AddRange(calls);
            _graph.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionsChanged())));

            _graph.Group();
            Handlers.ApplyPolicies(_graph);

            Services.For<HandlerGraph>().Use(_graph);
            Services.For<IChannelGraph>().Use(_channels);

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
                Services.ForSingletonOf<IDelayedJobProcessor>().Use<InMemoryDelayedJobProcessor>();
            }

            return Services;
        }
    }
}
