using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Transports;
using Jasper.Transports.Local;
using Jasper.Transports.Stub;

namespace Jasper.Configuration
{
    public class TransportCollection : IEnumerable<ITransport>, IEndpoints
    {
        private readonly JasperOptions _parent;
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();

        public TransportCollection(JasperOptions parent)
        {
            _parent = parent;
            Add(new StubTransport());
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
            foreach (var protocol in transport.Protocols)
            {
                _transports.SmartAdd(protocol, transport);
            }
        }

        public T Get<T>() where T : ITransport, new()
        {
            var transport = _transports.Values.OfType<T>().FirstOrDefault();
            if (transport == null)
            {
                transport = new T();
                foreach (var protocol in transport.Protocols)
                {
                    _transports[protocol] = transport;
                }
            }

            return transport;
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
        public IListenerConfiguration DurableScheduledMessagesLocalQueue => LocalQueue(TransportConstants.Durable);
        public IList<ISubscriber> Subscribers { get;  } = new List<ISubscriber>();

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
