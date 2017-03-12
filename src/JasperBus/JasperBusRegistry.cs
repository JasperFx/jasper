using System;
using System.Collections.Generic;
using Jasper;
using JasperBus.Model;

namespace JasperBus
{
    public class JasperBusRegistry : JasperRegistry
    {
        public JasperBusRegistry()
        {
            UseFeature<ServiceBusFeature>();
        }

        public HandlerSource Handlers => Feature<ServiceBusFeature>().Handlers;

        public Policies Policies => Feature<ServiceBusFeature>().Policies;
    }

    public class Policies
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
        }
    }
}