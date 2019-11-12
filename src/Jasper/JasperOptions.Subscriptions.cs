using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Util;

namespace Jasper
{
    public partial class JasperOptions
    {
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        /// <summary>
        ///     Array of all subscription rules for publishing messages from this
        ///     application
        /// </summary>
        public Subscription[] Subscriptions
        {
            get => _subscriptions.ToArray();
            set
            {
                _subscriptions.Clear();
                if (value != null) _subscriptions.AddRange(value);
            }
        }

        /// <summary>
        ///     Add a single subscription
        /// </summary>
        /// <param name="subscription"></param>
        public void AddSubscription(Subscription subscription)
        {
            _subscriptions.Fill(subscription);
        }

        private readonly IList<ListenerSettings> _listeners = new List<ListenerSettings>();


        public ListenerSettings[] Listeners
        {
            get => _listeners.ToArray();
            set
            {
                _listeners.Clear();
                if (value != null) _listeners.AddRange(value);
            }
        }

        /// <summary>
        ///     Listen for messages at the given uri
        /// </summary>
        /// <param name="uri"></param>
        public IListenerSettings ListenForMessagesFrom(Uri uri)
        {
            var listener = _listeners.FirstOrDefault(x => x.Uri == uri);
            if (listener == null)
            {
                listener = new ListenerSettings
                {
                    Uri = uri
                };

                _listeners.Add(listener);
            }

            return listener;
        }

        /// <summary>
        ///     Establish a message listener to a known location and transport
        /// </summary>
        /// <param name="uriString"></param>
        public IListenerSettings ListenForMessagesFrom(string uriString)
        {
            return ListenForMessagesFrom(uriString.ToUri());
        }

    }
}
