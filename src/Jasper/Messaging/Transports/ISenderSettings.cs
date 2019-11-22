using System.Collections.Generic;
using Jasper.Configuration;

namespace Jasper.Messaging.Transports
{
    public interface ISenderSettings
    {
        /// <summary>
        /// Force any messages enqueued to be sent by this sender to be durable
        /// </summary>
        /// <returns></returns>
        ISenderSettings Durably();

        /// <summary>
        /// By default, messages on this sender will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        ISenderSettings Lightweight();



        /// <summary>
        /// Add a message(s) subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        ISenderSettings Subscribe(Subscription subscription);
    }

    public abstract class SenderSettings<TSelf> : ISenderSettings where TSelf : ISenderSettings
    {
        private bool _isDurable;

        public bool IsDurable()
        {
            return _isDurable;
        }

        protected abstract TSelf returnThis();

        /// <summary>
        /// Force any messages enqueued to be sent by this sender to be durable
        /// </summary>
        /// <returns></returns>
        public TSelf Durably()
        {
            _isDurable = true;
            return returnThis();
        }

        /// <summary>
        /// By default, messages on this sender will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        public TSelf Lightweight()
        {
            _isDurable = false;
            return returnThis();
        }

        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        public TSelf Subscribe(Subscription subscription)
        {
            _subscriptions.Add(subscription);
            return returnThis();
        }

        public IEnumerable<Subscription> Subscriptions()
        {
            return _subscriptions;
        }

        ISenderSettings ISenderSettings.Durably()
        {
            return Durably();
        }

        ISenderSettings ISenderSettings.Lightweight()
        {
            return Lightweight();
        }

        ISenderSettings ISenderSettings.Subscribe(Subscription subscription)
        {
            return Subscribe(subscription);
        }
    }
}
