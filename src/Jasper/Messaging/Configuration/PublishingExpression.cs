using System;
using System.Reflection;
using Jasper.Configuration;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports;
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

        public void AllMessagesTo(string uriString)
        {
            AllMessagesTo(uriString.ToUri());
        }

        public void AllMessagesTo(Uri uri)
        {
            _parent.Transports.Subscribe(uri, new Subscription
            {
                Scope = RoutingScope.All
            });
        }


        /// <summary>
        ///     Directs Jasper to try to publish all messages locally even if there are other
        ///     subscribers for the message type
        /// </summary>
        public void AllMessagesLocally()
        {
            AllMessagesTo(TransportConstants.LocalUri);
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

            public void To(Uri address)
            {
                var subscription = new Subscription
                {
                    Match = _match,
                    Scope = _routingScope
                };

                _parent.Transports.Subscribe(address, subscription);
            }

            public void To(string address)
            {
                To(address.ToUri());
            }


            /// <summary>
            ///     Publishes the matching messages locally to the default
            ///     local queue
            /// </summary>
            public void Locally()
            {
                var subscription = new Subscription
                {
                    Scope = _routingScope, Match = _match
                };

                _parent.Transports.Subscribe(TransportConstants.LocalUri, subscription);
            }

            /// <summary>
            ///     Publish the designated message types using Jasper's lightweight
            ///     TCP transport locally to the designated port number
            /// </summary>
            /// <param name="port"></param>
            /// <exception cref="NotImplementedException"></exception>
            public void ToPort(int port)
            {
                To($"tcp://localhost:{port}".ToUri());
            }

            /// <summary>
            ///     Publish the designated message types using Jasper's built in
            ///     TCP transport with durable message persistence
            /// </summary>
            /// <param name="port"></param>
            public void DurablyToPort(int port)
            {
                To($"tcp://localhost:{port}/durable");
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
            /// <param name="serverName"></param>
            /// <param name="port"></param>
            public void ToServerAndPort(string serverName, int port)
            {
                To($"tcp://{serverName}:{port}");
            }

            public void ToStub(string queueName)
            {
                To("stub://" + queueName);
            }

            /// <summary>
            ///     Send messages to the named local queue with durable message
            ///     persistence
            /// </summary>
            /// <param name="queueName"></param>
            /// <returns></returns>
            public IListenerConfiguration DurablyToLocalQueue(string queueName)
            {
                return ToLocalQueue(queueName).Durably();
            }
        }
    }
}
