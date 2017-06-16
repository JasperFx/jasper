using System.Collections.Generic;
using Baseline;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;

namespace Jasper.Bus.Configuration
{
    public class Policies : IHasErrorHandlers
    {
        private readonly IList<IHandlerPolicy> _globals = new List<IHandlerPolicy>();

        // TODO -- have a Local option later
        public void Global<T>() where T : IHandlerPolicy, new()
        {
            Global(new T());
        }

        public void Global(IHandlerPolicy policy)
        {
            _globals.Add(policy);
        }


        internal void Apply(HandlerGraph graph)
        {
            foreach (var policy in _globals)
            {
                policy.Apply(graph);
            }

            graph.ErrorHandlers.AddRange(this.As<IHasErrorHandlers>().ErrorHandlers);
        }

        IList<IErrorHandler> IHasErrorHandlers.ErrorHandlers { get; } = new List<IErrorHandler>();
    }
}