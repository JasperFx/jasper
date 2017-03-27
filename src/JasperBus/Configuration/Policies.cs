using System.Collections.Generic;
using Baseline;
using JasperBus.ErrorHandling;
using JasperBus.Model;

namespace JasperBus.Configuration
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