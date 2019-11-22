using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Messaging.Transports.Stub;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Configuration
{
    public class TransportCollection : IEnumerable<ITransport>
    {
        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();

        public TransportCollection()
        {
            Add(new TcpTransport());
            Add(new StubTransport());
            Add(new LocalTransport());
        }

        public ITransport TransportForScheme(string scheme)
        {
            return _transports[scheme];
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

        public void Subscribe(Subscription subscription)
        {
            var transport = TransportForScheme(subscription.Uri.Scheme);
            if (transport == null)
            {
                throw new InvalidOperationException($"Unknown Transport scheme '{transport.Protocol}'");
            }

            transport.Subscribe(subscription);
        }

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        /// <param name="uri"></param>
        public IListenerSettings ListenForMessagesFrom(Uri uri)
        {
            if (_transports.TryGetValue(uri.Scheme, out var transport))
            {
                return transport.ListenTo(uri);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Unknown transport of type '{uri.Scheme}'");
            }
        }

        /// <summary>
        ///     Directs Jasper to set up an incoming listener for the given Uri
        /// </summary>
        public IListenerSettings ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(new Uri(uriString));
        }

        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     fast, but non-durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public IListenerSettings LightweightListenerAt(int port)
        {
            return Get<TcpTransport>().ListenTo($"tcp://localhost:{port}".ToUri());
        }

        /// <summary>
        ///     Directs the application to listen at the designated port in a
        ///     durable way
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="port"></param>
        public IListenerSettings DurableListenerAt(int port)
        {
            return Get<TcpTransport>().ListenTo($"tcp://localhost:{port}".ToUri()).Durably();
        }

        public IListenerSettings LocalQueue(string queueName)
        {
            return Get<LocalTransport>().ListenTo($"local://{queueName}".ToUri());
        }
    }
}
