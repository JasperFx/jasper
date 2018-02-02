using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BlueMilk;
using BlueMilk.Codegen;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Scheduled;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Configuration;
using Jasper.Http.Transport;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Bus
{
    public class ServiceBusFeature : IFeature
    {
        private readonly ChannelGraph _channels = new ChannelGraph();
        private readonly LocalWorkerSender _localWorker = new LocalWorkerSender();


        public readonly CapabilityGraph Capabilities = new CapabilityGraph();

        public readonly ServiceRegistry Services = new ServiceBusRegistry();

        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();


        public BusSettings Settings { get; } = new BusSettings();

        public void Dispose()
        {
        }

        Task<ServiceRegistry> IFeature.Bootstrap(JasperRegistry registry, PerfTimer timer)
        {
            return bootstrap(registry, timer);
        }

        void IFeature.Activate(JasperRuntime runtime, GenerationRules generation, PerfTimer timer)
        {
            timer.MarkStart("ServiceBusFeature.Activate");




            runtime.Get<ServiceBusActivator>()
                .Activate(_graph, Capabilities, runtime, _channels, _localWorker, timer, generation, Settings)
                .Wait();


            timer.MarkFinished("ServiceBusFeature.Activate");
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            var transports = runtime.Get<ITransport[]>()
                .Where(x => Settings.StateFor(x.Protocol) == TransportState.Enabled);

            foreach (var transport in transports) transport.Describe(writer);

            writer.WriteLine();
            foreach (var channel in _channels.AllKnownChannels())
                writer.WriteLine($"Active sending agent to {channel.Uri}");

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


        private async Task<ServiceRegistry> bootstrap(JasperRegistry registry, PerfTimer timer)
        {
            timer.MarkStart("ServiceBusFeature.Bootstrap");

            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            _graph = new HandlerGraph();
            _graph.AddRange(calls);
            _graph.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionsChanged())));

            _graph.Group();
            Handlers.ApplyPolicies(_graph);

            Services.AddSingleton(_graph);
            Services.AddSingleton<IChannelGraph>(_channels);
            Services.AddSingleton<ILocalWorkerSender>(_localWorker);

            Services.AddTransient<ServiceBusActivator>();


            if (registry.Logging.UseConsoleLogging)
            {
                Services.For<IMessageLogger>().Use<ConsoleMessageLogger>();
                Services.For<ITransportLogger>().Use<ConsoleTransportLogger>();
            }

            Services.ForSingletonOf<IScheduledJobProcessor>().UseIfNone<InMemoryScheduledJobProcessor>();

            timer.MarkFinished("ServiceBusFeature.Bootstrap");

            return Services;
        }
    }
}
