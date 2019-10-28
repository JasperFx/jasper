using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Jasper.Messaging.Transports;
using Lamar;
using LamarCodeGeneration;
using LamarCompiler;

namespace Jasper.Messaging
{
    public class MessagingConfiguration
    {
        public MessagingConfiguration()
        {
            Handling.GlobalPolicy<SagaFramePolicy>();
        }

        public HandlerConfiguration Handling => Graph.Configuration;


        public SubscriberGraph Subscribers { get; } = new SubscriberGraph();

        public LoopbackWorkerSender LocalWorker { get; } = new LoopbackWorkerSender();


        public HandlerGraph Graph { get; } = new HandlerGraph();


        internal void StartCompiling(JasperRegistry registry)
        {
            Compiling = Handling.Source.FindCalls(registry).ContinueWith(t =>
            {
                var calls = t.Result;

                if (calls != null && calls.Any()) Graph.AddRange(calls);

                Graph.Group();
            });
        }

        internal Task Compiling { get; private set; }
    }
}
