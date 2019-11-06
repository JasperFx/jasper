using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Configuration
{
    public class Endpoint
    {
        public string Name { get; set; }

        /// <summary>
        /// The outgoing address to send matching messages
        /// </summary>
        public Uri Uri { get; set; }

        public bool IsDurable { get; set; }

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public int BatchSize { get; set; }


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
    }
}
