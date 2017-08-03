using System;
using System.Reflection;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using StructureMap.TypeRules;

namespace Jasper.Bus.Configuration
{
    public class MessagesExpression
    {
        private readonly ServiceBusFeature _bus;

        public MessagesExpression(ServiceBusFeature bus)
        {
            _bus = bus;
        }

        public HandlerSource Handlers => _bus.Handlers;
        public Policies Policies => _bus.Policies;

        public DelayedJobExpression DelayedProcessing => new DelayedJobExpression(_bus);

        public IHasErrorHandlers ErrorHandling => Policies;

        public SendExpression Send<T>()
        {
            return new SendExpression(_bus, new SingleTypeRoutingRule<T>());
        }

        public SendExpression SendMatching(string description, Func<Type, bool> filter)
        {
            return new SendExpression(_bus, new LambdaRoutingRule(description, filter));
        }

        public SendExpression SendFromNamespace(string @namespace)
        {
            return new SendExpression(_bus, new NamespaceRule(@namespace));
        }

        public SendExpression SendFromNamespaceContaining<T>()
        {
            return SendFromNamespace(typeof(T).Namespace);
        }

        public SendExpression SendFromAssembly(Assembly assembly)
        {
            return new SendExpression(_bus, new AssemblyRule(assembly));
        }

        public SendExpression SendFromAssemblyContaining<T>()
        {
            return SendFromAssembly(typeof(T).GetAssembly());
        }

        public class SendExpression
        {
            private readonly ServiceBusFeature _bus;
            private readonly IRoutingRule _routing;

            public SendExpression(ServiceBusFeature bus, IRoutingRule routing)
            {
                _bus = bus;
                _routing = routing;
            }

            public SendExpression To(Uri address)
            {
                _bus.Channels[address].Rules.Add(_routing);
                return this;
            }

            public SendExpression To(string address)
            {
                return To(address.ToUri());
            }
        }
    }
}
