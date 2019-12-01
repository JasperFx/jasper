using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Jasper.Configuration;

namespace Jasper.Messaging.Configuration
{
    public interface IEndpoint
    {
        /// <summary>
        ///     Force any messages enqueued to be sent by this sender to be durable
        /// </summary>
        /// <returns></returns>
        IEndpoint Durably();

        /// <summary>
        ///     By default, messages on this sender will not be persisted until
        ///     being successfully handled
        /// </summary>
        /// <returns></returns>
        IEndpoint Lightweight();


        /// <summary>
        ///     Add a message(s) subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        IEndpoint Subscribe(Subscription subscription);


        IEndpoint ListenForMessages(Action<ExecutionDataflowBlockOptions> configure = null);
    }

    public abstract class Endpoint<TSelf> : IEndpoint where TSelf : IEndpoint
    {
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        private bool _isDurable;
        private bool _isListening;

        /// <summary>
        ///     Descriptive Name for this listener. Optional.
        /// </summary>
        public string Name { get; set; }

        public Uri Uri { get; protected set; }

        public ExecutionDataflowBlockOptions ExecutionOptions { get; set; } = new ExecutionDataflowBlockOptions();

        IEndpoint IEndpoint.ListenForMessages(Action<ExecutionDataflowBlockOptions> configure = null)
        {
            return ListenForMessages(configure);
        }

        IEndpoint IEndpoint.Durably()
        {
            return Durably();
        }

        IEndpoint IEndpoint.Lightweight()
        {
            return Lightweight();
        }

        IEndpoint IEndpoint.Subscribe(Subscription subscription)
        {
            return Subscribe(subscription);
        }

        public abstract TSelf Parse(Uri uri);

        /// <summary>
        ///     Should this transport endpoint be wrapped
        ///     with persistent, durable messaging?
        /// </summary>
        public bool IsDurable()
        {
            return _isDurable;
        }

        public bool IsListener()
        {
            return _isListening;
        }

        protected abstract TSelf returnThis();

        /// <summary>
        ///     Force any messages enqueued to be sent by this sender to be durable
        /// </summary>
        /// <returns></returns>
        public TSelf Durably()
        {
            _isDurable = true;
            return returnThis();
        }

        /// <summary>
        ///     By default, messages on this sender will not be persisted until
        ///     being successfully handled
        /// </summary>
        /// <returns></returns>
        public TSelf Lightweight()
        {
            _isDurable = false;
            return returnThis();
        }

        public TSelf Subscribe(Subscription subscription)
        {
            _subscriptions.Add(subscription);
            return returnThis();
        }

        public TSelf ListenForMessages(Action<ExecutionDataflowBlockOptions> configure = null)
        {
            _isListening = true;
            return returnThis();
        }

        public IEnumerable<Subscription> Subscriptions()
        {
            return _subscriptions;
        }
    }
}
