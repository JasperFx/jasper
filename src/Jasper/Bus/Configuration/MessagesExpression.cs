using System;
using System.Reflection;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
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

        public Policies Policies => _bus.Policies;

        public DelayedJobExpression DelayedProcessing => new DelayedJobExpression(_bus);

        public NoRouteBehavior NoRouteBehavior
        {
            get => _bus.Settings.NoRouteBehavior;
            set => _bus.Settings.NoRouteBehavior = value;
        }


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

            public ISubscriberAddress To(Uri address)
            {
                var subscriberAddress = _bus.Settings.SendTo(address);
                subscriberAddress.Rules.Add(_routing);

                return subscriberAddress;
            }

            public ISubscriberAddress To(string address)
            {
                return To(address.ToUri());
            }
        }

        public ISubscriberAddress SendAllMessagesTo(string uriString)
        {
            return SendAllMessagesTo(uriString.ToUri());
        }

        private ISubscriberAddress SendAllMessagesTo(Uri uri)
        {
            return SendMatching("all messages", _ => true).To(uri);
        }
    }


}
