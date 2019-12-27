using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Tcp;

namespace Jasper.Configuration
{
    public class TransportCollection : IEnumerable<ITransport>, IEndpoints
    {
        private readonly JasperOptions _parent;
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();

        public TransportCollection(JasperOptions parent)
        {
            _parent = parent;
            Add(new TcpTransport());
            Add(new LocalTransport());
        }


        public ITransport TransportForScheme(string scheme)
        {
            return _transports.TryGetValue(scheme.ToLowerInvariant(), out var transport)
                ? transport
                : null;
        }

        public void Add(ITransport transport)
        {
            _transports.SmartAdd(transport.Protocol, transport);
        }

        public T Get<T>() where T : ITransport, new()
        {
            return _transports.Values.OfType<T>().FirstOrDefault();
        }

        public IEnumerator<ITransport> GetEnumerator()
        {
            return _transports.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IListenerConfiguration IEndpoints.LocalQueue(string queueName)
        {
            return LocalQueue(queueName);
        }

        public Endpoint TryGetEndpoint(Uri uri)
        {
            return findTransport(uri).TryGetEndpoint(uri);
        }

        private ITransport findTransport(Uri uri)
        {
            var transport = TransportForScheme(uri.Scheme);
            if (transport == null)
            {
                throw new InvalidOperationException($"Unknown Transport scheme '{uri.Scheme}'");
            }

            return transport;
        }

        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            return findTransport(uri).GetOrCreateEndpoint(uri);
        }


        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        public IListenerConfiguration ListenForMessagesFrom(Uri uri)
        {
            var settings = findTransport(uri).ListenTo(uri);
            return new ListenerConfiguration(settings);
        }

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        public IListenerConfiguration ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(new Uri(uriString));
        }

        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     fast, but non-durable way
        /// </summary>
        /// <param name="port"></param>
        public IListenerConfiguration ListenAtPort(int port)
        {
            var settings = Get<TcpTransport>().ListenTo(TcpEndpoint.ToUri(port));
            return new ListenerConfiguration(settings);
        }

        public void Publish(Action<PublishingExpression> configuration)
        {
            var expression = new PublishingExpression(this);
            configuration(expression);
            expression.AttachSubscriptions();
        }

        public IPublishToExpression PublishAllMessages()
        {
            var expression = new PublishingExpression(this)
            {
                AutoAddSubscriptions = true
            };

            expression.AddSubscriptionForAllMessages();
            return expression;
        }

        public IListenerConfiguration LocalQueue(string queueName)
        {
            var settings = Get<LocalTransport>().QueueFor(queueName);
            return new ListenerConfiguration(settings);
        }

        public IListenerConfiguration DefaultLocalQueue => LocalQueue(TransportConstants.Default);
        public void StubAllExternallyOutgoingEndpoints()
        {
            _parent.Advanced.StubAllOutgoingExternalSenders = true;
        }

        public Endpoint[] AllEndpoints()
        {
            return _transports.Values.SelectMany(x => x.Endpoints()).ToArray();
        }

    }
}
