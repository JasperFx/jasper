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
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Internals;
using Jasper.Internals.Codegen;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
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

        public GenerationRules Generation { get; } = new GenerationRules("JasperBus.Generated");

        public BusSettings Settings { get; } = new BusSettings();

        public bool DelayedJobsRunInMemory { get; set; } = true;

        public readonly ServiceRegistry Services = new ServiceBusRegistry();

        public void Dispose()
        {
        }

        Task<ServiceRegistry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        Task IFeature.Activate(JasperRuntime runtime, GenerationRules generation)
        {
            _graph.Compile(generation, runtime);

            return runtime.Get<ServiceBusActivator>().Activate(_graph, Capabilities, runtime, _channels);
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            var transports = runtime.Get<ITransport[]>();
            foreach (var transport in transports.Where(x => x.State == TransportState.Enabled))
            {
                transport.Describe(writer);
            }

            if (runtime.Registry.Logging.Verbose)
            {
                writer.WriteLine("Handles messages:");

                var longestMessageName = _graph.Chains.Select(x => x.MessageType.FullName.Length).Max() + 2;

                foreach (var chain in _graph.Chains)
                {
                    var messageName = chain.MessageType.FullName.PadLeft(longestMessageName);
                    var handlers = chain.Handlers.Select(x => x.ToString()).Join(", ");


                    writer.WriteLine($"{messageName}: {handlers}");
                }

                writer.WriteLine();
            }
        }



        private async Task<ServiceRegistry> bootstrap(JasperRegistry registry)
        {
            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            _graph = new HandlerGraph();
            _graph.AddRange(calls);
            _graph.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionsChanged())));

            _graph.Group();
            Handlers.ApplyPolicies(_graph);

            Services.AddSingleton(_graph);
            Services.AddSingleton<IChannelGraph>(_channels);

            Services.AddTransient<ServiceBusActivator>();


            if (registry.Logging.UseConsoleLogging)
            {
                Services.For<IBusLogger>().Use<ConsoleBusLogger>();
            }

            Services.ForSingletonOf<IDelayedJobProcessor>().UseIfNone<InMemoryDelayedJobProcessor>();

            return Services;
        }
    }
}
