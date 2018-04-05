using System;
using System.Reflection;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Messaging.Configuration
{
    public class PublishingExpression
    {
        private readonly MessagingConfiguration _bus;

        internal PublishingExpression(MessagingConfiguration bus)
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

        public ISubscriberAddress AllMessagesTo(Uri uri)
        {
            return MessagesMatching("all messages", _ => true).To(uri);
        }

        public class MessageTrackExpression
        {
            private readonly MessagingConfiguration _bus;
            private readonly IRoutingRule _routing;

            internal MessageTrackExpression(MessagingConfiguration bus, IRoutingRule routing)
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

            /// <summary>
            /// Customize how a message of this type is sent by modifying
            /// the outgoing Envelope
            /// </summary>
            /// <param name="customization"></param>
            /// <returns></returns>
            /// <exception cref="NotImplementedException"></exception>
            public MessageTrackExpression Customize(Action<Envelope> customization)
            {
                var rule = new MessageTypeRule(type => _routing.Matches(type), customization);
                _bus.Settings.MessageTypeRules.Add(rule);

                return this;
            }
        }
    }
}
