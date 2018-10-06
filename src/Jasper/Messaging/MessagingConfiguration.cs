using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Lamar.Codegen;
using Lamar.Util;

namespace Jasper.Messaging
{
    public class MessagingConfiguration
    {
        public HandlerConfiguration Handling { get; }


        public MessagingSettings Settings { get; } = new MessagingSettings();

        public SubscriberGraph Subscribers { get; } = new SubscriberGraph();

        public LocalWorkerSender LocalWorker { get; } = new LocalWorkerSender();



        public HandlerGraph Graph { get;  } = new HandlerGraph();


        public MessagingConfiguration()
        {
            Handling = new HandlerConfiguration(Graph, Settings);

            Handling.GlobalPolicy<SagaFramePolicy>();
        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            var transports = runtime.Get<ITransport[]>()
                .Where(x => Settings.StateFor(x.Protocol) == TransportState.Enabled);

            foreach (var transport in transports) transport.Describe(writer);

            writer.WriteLine();
            foreach (var channel in Subscribers.AllKnown())
                writer.WriteLine($"Active sending agent to {channel.Uri}");

            writer.WriteLine("Handles messages:");

            var longestMessageName = Graph.Chains.Select(x => x.MessageType.NameInCode().Length).Max() + 2;

            foreach (var chain in Graph.Chains)
            {
                var messageName = chain.MessageType.NameInCode().PadLeft(longestMessageName);
                var handlers = chain.Handlers.Select(x => x.ToString()).Join(", ");


                writer.WriteLine($"{messageName}: {handlers}");
            }

            writer.WriteLine();
        }

        internal Task CompileHandlers(JasperRegistry registry, PerfTimer timer)
        {
            return Handling.Source.FindCalls(registry).ContinueWith(t =>
            {
                timer.Record("Compile Handlers", () =>
                {
                    var calls = t.Result;

                    if (calls != null && calls.Any()) Graph.AddRange(calls);

                    Graph.Group();
                    Handling.ApplyPolicies(Graph, registry.CodeGeneration);

                });


            });
        }

    }
}
