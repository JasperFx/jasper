using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Configuration;

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

    }
}
