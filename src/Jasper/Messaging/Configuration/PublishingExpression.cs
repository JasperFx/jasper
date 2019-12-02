using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
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

        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        internal PublishingExpression(JasperOptions parent)
        {
            _parent = parent;
        }


        public PublishingExpression Message<T>()
        {
            return Message(typeof(T));
        }

        public PublishingExpression Message(Type type)
        {
            _subscriptions.Add(Subscription.ForType(type));
            return this;
        }

        public PublishingExpression MessagesFromNamespace(string @namespace)
        {
            _subscriptions.Add(new Subscription
            {
                Match = @namespace,
                Scope = RoutingScope.Namespace
            });

            return this;
        }

        public PublishingExpression MessagesFromNamespaceContaining<T>()
        {
            return MessagesFromNamespace(typeof(T).Namespace);
        }

        public PublishingExpression MessagesFromAssembly(Assembly assembly)
        {
            _subscriptions.Add(new Subscription(assembly));
            return this;
        }

        public PublishingExpression MessagesFromAssemblyContaining<T>()
        {
            return MessagesFromAssembly(typeof(T).Assembly);
        }

        public ISubscriberConfiguration AllMessagesTo(string uriString)
        {
            return AllMessagesTo(uriString.ToUri());
        }

        public ISubscriberConfiguration AllMessagesTo(Uri uri)
        {
            if (_subscriptions.Any())
            {
                throw new InvalidOperationException($"{nameof(AllMessagesTo)} is only valid if there are no other message matching rules in this expression");
            }

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

        /// <summary>
        /// All matching records are to be sent to the configured subscriber
        /// by Uri
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public ISubscriberConfiguration To(Uri address)
        {
            return _parent.Transports.Subscribe(address, _subscriptions.ToArray());
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

            var settings = _parent.Transports.Get<LocalTransport>().QueueFor(TransportConstants.Default);
            settings.Subscriptions.AddRange(_subscriptions);

            return new ListenerConfiguration(settings);
        }

        /// <summary>
        ///     Publish the designated message types using Jasper's lightweight
        ///     TCP transport locally to the designated port number
        /// </summary>
        /// <param name="port"></param>
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
            settings.Subscriptions.AddRange(_subscriptions);

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
