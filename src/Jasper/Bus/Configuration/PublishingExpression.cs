using System;
using System.Reflection;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Bus.Configuration
{
    public class PublishingExpression
    {
        private readonly ServiceBusFeature _bus;

        internal PublishingExpression(ServiceBusFeature bus)
        {
            _bus = bus;
        }


        public MessageTrackExpression Message<T>()
        {
            _bus.Capabilities.Publish(typeof(T));
            return new MessageTrackExpression(_bus, new SingleTypeRoutingRule<T>());
        }

        public MessageTrackExpression Message(Type type)
        {
            _bus.Capabilities.Publish(type);
            return new MessageTrackExpression(_bus, new LambdaRoutingRule(type.FullName, t => t == type));
        }

        public MessageTrackExpression MessagesMatching(string description, Func<Type, bool> filter)
        {
            _bus.Capabilities.PublishesMessagesMatching(filter);
            return new MessageTrackExpression(_bus, new LambdaRoutingRule(description, filter));
        }

        public MessageTrackExpression MessagesMatching(Func<Type, bool> filter)
        {
            _bus.Capabilities.PublishesMessagesMatching(filter);
            return new MessageTrackExpression(_bus, new LambdaRoutingRule("User supplied filter", filter));
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
            return MessagesFromAssembly(typeof(T).Assembly);
        }

        public ISubscriberAddress AllMessagesTo(string uriString)
        {
            return AllMessagesTo(uriString.ToUri());
        }

        private ISubscriberAddress AllMessagesTo(Uri uri)
        {
            return MessagesMatching("all messages", _ => true).To(uri);
        }

        public class MessageTrackExpression
        {
            private readonly ServiceBusFeature _bus;
            private readonly IRoutingRule _routing;

            internal MessageTrackExpression(ServiceBusFeature bus, IRoutingRule routing)
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
    }
}
