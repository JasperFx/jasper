using System;
using System.Reflection;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Settings;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;

namespace Jasper.Messaging.Configuration
{
    public class PublishingExpression
    {
        private readonly MessagingConfiguration _bus;
        private readonly JasperSettings _settings;

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

        /// <summary>
        ///     Directs Jasper to try to publish all messages locally even if there are other
        ///     subscribers for the message type
        /// </summary>
        public void AllMessagesLocally()
        {
            var rule = Subscription.All();
            _settings.Messaging(x => x.LocalPublishing.Add(rule));
        }

        public class MessageTrackExpression
        {
            private readonly MessagingConfiguration _bus;
            private readonly string _match;
            private readonly RoutingScope _routingScope;
            private readonly JasperSettings _settings;

            internal MessageTrackExpression(JasperSettings settings, MessagingConfiguration bus,
                RoutingScope routingScope, string match)
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
            ///     Publishes the matching messages locally in  addition to any other subscriber rules
            /// </summary>
            public void Locally()
            {
                _settings.Messaging(x =>
                {
                    x.LocalPublishing.Add(new Subscription
                    {
                        Scope = _routingScope, Match = _match
                    });
                });
            }

            /// <summary>
            /// Publish messages matching this rule to the Uri value
            /// found in IConfiguration[configKey]
            /// </summary>
            /// <param name="configKey">The configuration key that holds the designated Uri</param>
            public void ToUriValueInConfig(string configKey)
            {
                _settings.Alter((Action<WebHostBuilderContext, JasperOptions>) ((c, options) =>
                {
                    var subscription = new Subscription
                    {
                        Match = _match,
                        Scope = _routingScope,
                        Uri = c.Configuration.TryGetUri(configKey)
                    };

                    options.AddSubscription(subscription);
                }));
            }
        }

        /// <summary>
        /// Send all messages published through this application to the Uri
        /// value in configuration with the designated key
        /// </summary>
        /// <param name="configKey">The value of IConfiguration[configKey] to find the Uri</param>
        public void AllMessagesToUriValueInConfig(string configKey)
        {
            _settings.Alter<JasperOptions>((c, options) =>
            {
                var uri = c.Configuration.TryGetUri(configKey);
                options.AddSubscription(Subscription.All(uri));
            });
        }
    }
}
