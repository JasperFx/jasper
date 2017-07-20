using System;
using System.Reflection;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using StructureMap.TypeRules;

namespace Jasper.Bus
{
    public class MessagesExpression
    {
        private readonly ServiceBusFeature _bus;

        public MessagesExpression(ServiceBusFeature bus)
        {
            _bus = bus;
        }

        public SendExpression SendMessage<T>()
        {
            return new SendExpression(_bus, new SingleTypeRoutingRule<T>());
        }

        public SendExpression SendMessages(string description, Func<Type, bool> filter)
        {
            return new SendExpression(_bus, new LambdaRoutingRule(description, filter));
        }

        public SendExpression SendMessagesInNamespace(string @namespace)
        {
            return new SendExpression(_bus, new NamespaceRule(@namespace));
        }

        public SendExpression SendMessagesInNamespaceContaining<T>()
        {
            return SendMessagesInNamespace(typeof(T).Namespace);
        }

        public SendExpression SendMessagesFromAssembly(Assembly assembly)
        {
            return new SendExpression(_bus, new AssemblyRule(assembly));
        }

        public SendExpression SendMessagesFromAssemblyContaining<T>()
        {
            return SendMessagesFromAssembly(typeof(T).GetAssembly());
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