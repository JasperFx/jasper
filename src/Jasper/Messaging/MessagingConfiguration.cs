using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Jasper.Messaging.Transports;
using LamarCompiler;

namespace Jasper.Messaging
{
    public class MessagingConfiguration
    {
        public MessagingConfiguration()
        {
            Handling = new HandlerConfiguration(Graph);

            Handling.GlobalPolicy<SagaFramePolicy>();
        }

        public HandlerConfiguration Handling { get; }


        public SubscriberGraph Subscribers { get; } = new SubscriberGraph();

        public LocalWorkerSender LocalWorker { get; } = new LocalWorkerSender();


        public HandlerGraph Graph { get; } = new HandlerGraph();

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            var settings = runtime.Get<JasperOptions>();

            var transports = runtime.Get<ITransport[]>()
                .Where(x => settings.StateFor(x.Protocol) == TransportState.Enabled);

            foreach (var transport in transports) transport.Describe(writer);

            writer.WriteLine();
            foreach (var channel in Subscribers.AllKnown())
                writer.WriteLine($"Active sending agent to {channel.Uri}");

            if (Graph.Chains.Any())
            {
                writer.WriteLine("Handles messages:");

                var longestMessageName = Graph.Chains.Select(x => x.MessageType.NameInCode().Length).Max() + 2;

                foreach (var chain in Graph.Chains)
                {
                    var messageName = chain.MessageType.NameInCode().PadLeft(longestMessageName);
                    var handlers = chain.Handlers.Select(x => x.ToString()).Join(", ");


                    writer.WriteLine($"{messageName}: {handlers}");
                }
            }


            writer.WriteLine();
        }

        internal void StartCompiling(JasperOptionsBuilder registry)
        {
            Compiling = Handling.Source.FindCalls(registry).ContinueWith(t =>
            {
                var calls = t.Result;

                if (calls != null && calls.Any()) Graph.AddRange(calls);

                Graph.Group();
                Handling.ApplyPolicies(Graph, registry.CodeGeneration);
            });
        }

        internal Task Compiling { get; private set; }
    }
}
