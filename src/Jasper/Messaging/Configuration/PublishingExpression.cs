using System;
using System.Reflection;
using Jasper.Configuration;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Messaging.Configuration
{
    public class PublishingExpression
    {
        private readonly JasperOptions _parent;

        internal PublishingExpression(JasperOptions parent)
        {
            _parent = parent;
        }


        public MessageTrackExpression Message<T>()
        {
            return Message(typeof(T));
        }

        public MessageTrackExpression Message(Type type)
        {
            return new MessageTrackExpression(_parent, RoutingScope.Type, type.ToMessageTypeName());
        }

        public MessageTrackExpression MessagesFromNamespace(string @namespace)
        {
            return new MessageTrackExpression(_parent, RoutingScope.Namespace, @namespace);
        }

        public MessageTrackExpression MessagesFromNamespaceContaining<T>()
        {
            return MessagesFromNamespace(typeof(T).Namespace);
        }

        public MessageTrackExpression MessagesFromAssembly(Assembly assembly)
        {
            return new MessageTrackExpression(_parent, RoutingScope.Assembly, assembly.GetName().Name);
        }

        public MessageTrackExpression MessagesFromAssemblyContaining<T>()
        {
            return MessagesFromAssembly(typeof(T).Assembly);
        }

        public ISubscriberConfiguration AllMessagesTo(string uriString)
        {
            return AllMessagesTo(uriString.ToUri());
        }

        public ISubscriberConfiguration AllMessagesTo(Uri uri)
        {
            return _parent.Transports.Subscribe(uri, new Subscription
            {
                Scope = RoutingScope.All
            });
        }


        /// <summary>
        ///     Directs Jasper to try to publish all messages locally even if there are other
        ///     subscribers for the message type
        /// </summary>
        public ISubscriberConfiguration AllMessagesLocally()
        {
            return AllMessagesTo(TransportConstants.LocalUri);
        }

        public class MessageTrackExpression
        {
            private readonly string _match;
            private readonly JasperOptions _parent;
            private readonly RoutingScope _routingScope;

            internal MessageTrackExpression(JasperOptions parent,
                RoutingScope routingScope, string match)
            {
                _parent = parent;
                _routingScope = routingScope;
                _match = match;
            }

            public ISubscriberConfiguration To(Uri address)
            {
                var subscription = new Subscription
                {
                    Match = _match,
                    Scope = _routingScope
                };

                return _parent.Transports.Subscribe(address, subscription);
            }

            /// <summary>
            /// Send all the matching messages to the designated Uri string
            /// </summary>
            /// <param name="uriString"></param>
            /// <returns></returns>
            public ISubscriberConfiguration To(string uriString)
            {
                return To(uriString.ToUri());
            }


            /// <summary>
            ///     Publishes the matching messages locally to the default
            ///     local queue
            /// </summary>
            public IListenerConfiguration Locally()
            {
                var subscription = new Subscription
                {
                    Scope = _routingScope, Match = _match
                };

                var settings = _parent.Transports.Get<LocalTransport>().QueueFor(TransportConstants.Default);
                settings.Subscriptions.Add(subscription);

                return new ListenerConfiguration(settings);
            }

            /// <summary>
            ///     Publish the designated message types using Jasper's lightweight
            ///     TCP transport locally to the designated port number
            /// </summary>
            /// <param name="port"></param>
            /// <exception cref="NotImplementedException"></exception>
            public ISubscriberConfiguration ToPort(int port)
            {
                var uri = TcpEndpoint.ToUri(port);
                return To(uri);
            }

            /// <summary>
            ///     Publish the designated message types to the named
            ///     local queue
            /// </summary>
            /// <param name="queueName"></param>
            /// <returns></returns>
            public IListenerConfiguration ToLocalQueue(string queueName)
            {
                var settings = _parent.LocalQueues.ByName(queueName);
                settings.Subscriptions.Add(new Subscription
                {
                    Scope = _routingScope, Match = _match
                });

                return new ListenerConfiguration(settings);
            }

            /// <summary>
            ///     Publish messages using the TCP transport to the specified
            ///     server name and port
            /// </summary>
            /// <param name="hostName"></param>
            /// <param name="port"></param>
            public ISubscriberConfiguration ToServerAndPort(string hostName, int port)
            {
                var uri = TcpEndpoint.ToUri(port, hostName);
                return To(uri);
            }


            public void ToStub(string queueName)
            {
                // TODO -- get the Uri logic out of here
                To("stub://" + queueName);
            }
        }
    }
}
