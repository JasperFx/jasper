using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BlueMilk.Codegen;
using BlueMilk.Util;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper.Messaging
{
    public class MessagingConfiguration
    {
        public readonly CapabilityGraph Capabilities = new CapabilityGraph();


        public HandlerSource Handlers { get; } = new HandlerSource();


        public MessagingSettings Settings { get; } = new MessagingSettings();

        public ChannelGraph Channels { get; } = new ChannelGraph();

        public LocalWorkerSender LocalWorker { get; } = new LocalWorkerSender();

        public void Dispose()
        {
        }

        public HandlerGraph Graph { get;  } = new HandlerGraph();


        internal Task Activate(JasperRuntime runtime, GenerationRules generation, PerfTimer timer)
        {
            var activator = timer.Record("Building ServiceBusActivator", runtime.Get<IMessagingRoot>);

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
