using System;
using System.Reflection;
using Jasper;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Routing;
using StructureMap.TypeRules;

namespace JasperBus
{
    public class JasperBusRegistry : JasperRegistry
    {
        private readonly ServiceBusFeature _feature;

        public JasperBusRegistry()
        {
            UseFeature<ServiceBusFeature>();

            _feature = Feature<ServiceBusFeature>();
        }

        public HandlerSource Handlers => _feature.Handlers;

        public Policies Policies => _feature.Policies;

        public void ListenForMessagesFrom(Uri uri)
        {
            _feature.Channels[uri].Incoming = true;
        }

        public void ListenForMessagesFrom(string uriString)
        {
            ListenForMessagesFrom(uriString.ToUri());
        }

        public SendExpression SendMessage<T>()
        {
            return new SendExpression(this, new SingleTypeRoutingRule<T>());
        }

        public SendExpression SendMessages(string description, Func<Type, bool> filter)
        {
            return new SendExpression(this, new LambdaRoutingRule(description, filter));
        }

        public SendExpression SendMessagesInNamespace(string @namespace)
        {
            return new SendExpression(this, new NamespaceRule(@namespace));
        }

        public SendExpression SendMessagesInNamespaceContaining<T>()
        {
            return SendMessagesInNamespace(typeof(T).Namespace);
        }

        public SendExpression SendMessagesFromAssembly(Assembly assembly)
        {
            return new SendExpression(this, new AssemblyRule(assembly));
        }

        public SendExpression SendMessagesFromAssemblyContaining<T>()
        {
            return SendMessagesFromAssembly(typeof(T).GetAssembly());
        }

        public class SendExpression
        {
            private readonly JasperBusRegistry _parent;
            private readonly IRoutingRule _routing;

            public SendExpression(JasperBusRegistry parent, IRoutingRule routing)
            {
                _parent = parent;
                _routing = routing;
            }

            public SendExpression To(Uri address)
            {
                _parent._feature.Channels[address].Rules.Add(_routing);
                return this;
            }

            public SendExpression To(string address)
            {
                return To(address.ToUri());
            }
        }


    }
}