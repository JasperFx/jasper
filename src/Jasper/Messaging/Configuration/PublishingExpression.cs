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
            return new MessageTrackExpression(_bus, RoutingRule.ForType<T>());
        }

        public MessageTrackExpression Message(Type type)
        {
            return new MessageTrackExpression(_bus, RoutingRule.ForType(type));
        }

        public MessageTrackExpression MessagesFromNamespace(string @namespace)
        {
            return new MessageTrackExpression(_bus, new RoutingRule
            {
                Scope = RoutingScope.Namespace,
                Value = @namespace
            });

        }

        public MessageTrackExpression MessagesFromNamespaceContaining<T>()
        {
            return MessagesFromNamespace(typeof(T).Namespace);
        }

        public MessageTrackExpression MessagesFromAssembly(Assembly assembly)
        {

            return new MessageTrackExpression(_bus, new RoutingRule(assembly));
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
            var address = _bus.Settings.SendTo(uri);
            address.Rules.Add(RoutingRule.All());

            return address;
        }

        public class MessageTrackExpression
        {
            private readonly MessagingConfiguration _bus;
            private readonly RoutingRule _routing;

            internal MessageTrackExpression(MessagingConfiguration bus, RoutingRule routing)
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
            public MessageTrackExpression Customize(Action<Envelope> customization)
            {
                var rule = new MessageTypeRule(type => _routing.Matches(type), customization);
                _bus.Settings.MessageTypeRules.Add(rule);

                return this;
            }

            /// <summary>
            /// Publishes the matching messages locally in addition to any other subscriber rules
            /// </summary>
            public void Locally()
            {
                _bus.Settings.LocalPublishing.Add(_routing);
            }
        }

        /// <summary>
        /// Directs Jasper to try to publish all messages locally even if there are other
        /// subscribers for the message type
        /// </summary>
        public void AllMessagesLocally()
        {
            var rule = RoutingRule.All();
            _bus.Settings.LocalPublishing.Add(rule);
        }
    }
}
