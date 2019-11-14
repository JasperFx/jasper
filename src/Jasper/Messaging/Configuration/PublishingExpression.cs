﻿using System;
using System.Reflection;
using Jasper.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Transports;
using Jasper.Settings;
using Jasper.Util;
using Microsoft.Extensions.Hosting;

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

        [Obsolete]
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

            _parent.AddSubscription(subscription);

        }

        /// <summary>
        ///     Directs Jasper to try to publish all messages locally even if there are other
        ///     subscribers for the message type
        /// </summary>
        public void AllMessagesLocally()
        {
            AllMessagesTo(TransportConstants.LoopbackUri);
        }

        public class MessageTrackExpression
        {
            private readonly string _match;
            private readonly RoutingScope _routingScope;
            private readonly JasperOptions _parent;

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
                    Scope = _routingScope,
                    Uri = address
                };

                _parent.AddSubscription(subscription);
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
                var subscription = new Subscription
                {
                    Scope = _routingScope, Match = _match, Uri = TransportConstants.LoopbackUri
                };

                _parent.AddSubscription(subscription);
            }

        }


    }
}
