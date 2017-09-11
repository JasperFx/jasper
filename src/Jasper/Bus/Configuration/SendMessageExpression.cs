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
    public class SendMessageExpression
    {
        private readonly ServiceBusFeature _bus;

        public SendMessageExpression(ServiceBusFeature bus)
        {
            _bus = bus;
        }

        public DelayedJobExpression DelayedProcessing => new DelayedJobExpression(_bus);


        public MessageTrackExpression Message<T>()
        {
            return new MessageTrackExpression(_bus, new SingleTypeRoutingRule<T>());
        }

        public MessageTrackExpression MessagesMatching(string description, Func<Type, bool> filter)
        {
            return new MessageTrackExpression(_bus, new LambdaRoutingRule(description, filter));
        }

        public MessageTrackExpression MessagesFromNamespace(string @namespace)
        {
            return new MessageTrackExpression(_bus, new NamespaceRule(@namespace));
        }

        public MessageTrackExpression MessagesFromNamespaceContaining<T>()
        {
            return MessagesFromNamespace(typeof(T).Namespace);
        }

        public MessageTrackExpression MessagesFromAssembly(Assembly assembly)
        {
            return new MessageTrackExpression(_bus, new AssemblyRule(assembly));
        }

        public MessageTrackExpression MessagesFromAssemblyContaining<T>()
        {
            return MessagesFromAssembly(typeof(T).GetAssembly());
        }

        public class MessageTrackExpression
        {
            private readonly ServiceBusFeature _bus;
            private readonly IRoutingRule _routing;

            public MessageTrackExpression(ServiceBusFeature bus, IRoutingRule routing)
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

        public ISubscriberAddress AllMessagesTo(string uriString)
        {
            return AllMessagesTo(uriString.ToUri());
        }

        private ISubscriberAddress AllMessagesTo(Uri uri)
        {
            return MessagesMatching("all messages", _ => true).To(uri);
        }
    }


}
