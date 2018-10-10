using System;
using System.Reflection;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Settings;
using Jasper.Util;

namespace Jasper.Messaging.Configuration
{
    public class PublishingExpression
    {
        private readonly JasperSettings _settings;
        private readonly MessagingConfiguration _bus;

        internal PublishingExpression(JasperSettings settings, MessagingConfiguration bus)
        {
            _settings = settings;
            _bus = bus;
        }


        public MessageTrackExpression Message<T>()
        {
            return Message(typeof(T));
        }

        public MessageTrackExpression Message(Type type)
        {
            return new MessageTrackExpression(_settings, _bus, RoutingScope.Type, type.ToMessageTypeName());
        }

        public MessageTrackExpression MessagesFromNamespace(string @namespace)
        {
            return new MessageTrackExpression(_settings, _bus, RoutingScope.Namespace, @namespace);
        }

        public MessageTrackExpression MessagesFromNamespaceContaining<T>()
        {
            return MessagesFromNamespace(typeof(T).Namespace);
        }

        public MessageTrackExpression MessagesFromAssembly(Assembly assembly)
        {
            return new MessageTrackExpression(_settings, _bus, RoutingScope.Assembly, assembly.GetName().Name);
        }

        public MessageTrackExpression MessagesFromAssemblyContaining<T>()
        {
            return MessagesFromAssembly(typeof(T).Assembly);
        }

        public void AllMessagesTo(string uriString)
        {
            AllMessagesTo(uriString.ToUri());
        }

        public void AllMessagesTo(Uri uri)
        {
            var subscription = new Subscription
            {
                Scope = RoutingScope.All,
                Uri = uri
            };

            _settings.Messaging(x => x.AddSubscription(subscription));

        }

        public class MessageTrackExpression
        {
            private readonly RoutingScope _routingScope;
            private readonly string _match;
            private readonly JasperSettings _settings;
            private readonly MessagingConfiguration _bus;

            internal MessageTrackExpression(JasperSettings settings, MessagingConfiguration bus, RoutingScope routingScope, string match)
            {
                _settings = settings;
                _bus = bus;
                _routingScope = routingScope;
                _match = match;
            }

            public void To(Uri address)
            {
                var subscription = new Subscription
                {
                    Match = _match,
                    Scope = _routingScope,
                    Uri = address
                };

                _settings.Messaging(x => x.AddSubscription(subscription));
            }

            public void To(string address)
            {
                To(address.ToUri());
            }


            /// <summary>
            /// Publishes the matching messages locally in  addition to any other subscriber rules
            /// </summary>
            public void Locally()
            {
                _settings.Messaging(x =>
                {
                    x.LocalPublishing.Add(new Subscription()
                    {
                        Scope = _routingScope, Match = _match
                    });
                });


            }
        }

        /// <summary>
        /// Directs Jasper to try to publish all messages locally even if there are other
        /// subscribers for the message type
        /// </summary>
        public void AllMessagesLocally()
        {
            var rule = Subscription.All();
            _settings.Messaging(x => x.LocalPublishing.Add(rule));
        }
    }
}
