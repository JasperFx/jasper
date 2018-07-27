using System.Linq;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Lamar.Codegen.Frames;
using Marten;

namespace Jasper.Marten.Codegen
{
    public class FineGrainedSessionCreationPolicy : IHandlerPolicy
    {
        public const string OpenSessionMethodName = "OpenSession";

        public void Apply(HandlerGraph graph)
        {
            foreach (var chain in graph.Chains)
            {
                applyToChain(chain);
            }
        }

        private void applyToChain(HandlerChain chain)
        {
            foreach (var handler in chain.Handlers)
            {
                var method = handler.HandlerType.GetMethod(OpenSessionMethodName);
                if (method == null) continue;

                if (method.ReturnType != typeof(IDocumentSession)) continue;

                var @call = new MethodCall(handler.HandlerType, method);
                chain.Middleware.Add(@call);
            }
        }
    }
}
