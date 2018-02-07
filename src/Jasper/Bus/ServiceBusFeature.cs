using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BlueMilk;
using BlueMilk.Codegen;
using BlueMilk.Util;
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
    public class ServiceBusFeature
    {
        public readonly CapabilityGraph Capabilities = new CapabilityGraph();


        public HandlerSource Handlers { get; } = new HandlerSource();


        public BusSettings Settings { get; } = new BusSettings();

        public ChannelGraph Channels { get; } = new ChannelGraph();

        public LocalWorkerSender LocalWorker { get; } = new LocalWorkerSender();

        public void Dispose()
        {
        }

        public HandlerGraph Graph { get;  } = new HandlerGraph();


        internal Task Activate(JasperRuntime runtime, GenerationRules generation, PerfTimer timer)
        {
            var activator = timer.Record("Building ServiceBusActivator", runtime.Get<MessagingRoot>);

            return activator.Activate(LocalWorker, Capabilities, runtime, generation, timer);
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            var transports = runtime.Get<ITransport[]>()
                .Where(x => Settings.StateFor(x.Protocol) == TransportState.Enabled);

            foreach (var transport in transports) transport.Describe(writer);

            writer.WriteLine();
            foreach (var channel in Channels.AllKnownChannels())
                writer.WriteLine($"Active sending agent to {channel.Uri}");

            if (runtime.Registry.Logging.Verbose)
            {
                writer.WriteLine("Handles messages:");

                var longestMessageName = Graph.Chains.Select(x => x.MessageType.FullName.Length).Max() + 2;

                foreach (var chain in Graph.Chains)
                {
                    var messageName = chain.MessageType.FullName.PadLeft(longestMessageName);
                    var handlers = chain.Handlers.Select(x => x.ToString()).Join(", ");


                    writer.WriteLine($"{messageName}: {handlers}");
                }

                writer.WriteLine();
            }
        }

        internal Task CompileHandlers(JasperRegistry registry, PerfTimer timer)
        {
            return Handlers.FindCalls(registry).ContinueWith(t =>
            {
                timer.Record("Compile Handlers", () =>
                {
                    var calls = t.Result;

                    Graph.AddRange(calls);
                    Graph.Add(HandlerCall.For<SubscriptionsHandler>(x => x.Handle(new SubscriptionsChanged())));

                    Graph.Group();
                    Handlers.ApplyPolicies(Graph);

                });


            });
        }

    }
}
