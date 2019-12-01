using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public abstract class TransportBase : ITransport
    {
        public TransportBase(string protocol)
        {
            Protocol = protocol;
        }



        public string Protocol { get; }
        public Uri ReplyUri { get; protected set; }

        public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            var endpoints = _subscriptions.GroupBy(x => x.Uri);
            foreach (var endpoint in endpoints)
            {
                var sender = CreateSender(endpoint.Key, root.Settings.Cancellation, root);
                runtime.AddSubscriber(ReplyUri, sender, endpoint.ToArray());
            }
        }

        public void StartListeners(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var settings in _listeners)
            {
                var listener = createListener(settings, root);
                runtime.AddListener(listener, settings);
            }
        }

        protected abstract IListener createListener(Endpoint settings, IMessagingRoot root);

        public abstract ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root);

        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        public void Subscribe(Subscription subscription)
        {
            _subscriptions.Add(subscription);
        }

        private readonly LightweightCache<Uri, Endpoint> _listeners = new LightweightCache<Uri, Endpoint>(uri => new Endpoint{Uri = uri});

        public Endpoint ListenTo(Uri uri)
        {
            return _listeners[uri];
        }


        public void Dispose()
        {

        }

    }
}
