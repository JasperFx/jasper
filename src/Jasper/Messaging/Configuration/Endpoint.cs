using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Configuration
{
    public interface IEndpoint
    {
        /// <summary>
        /// Force any messages enqueued to be sent by this sender to be durable
        /// </summary>
        /// <returns></returns>
        IEndpoint Durably();

        /// <summary>
        /// By default, messages on this sender will not be persisted until
        /// being successfully handled
        /// </summary>
        /// <returns></returns>
        IEndpoint Lightweight();



        /// <summary>
        /// Add a message(s) subscription
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        IEndpoint Subscribe(Subscription subscription);


        IEndpoint ListenForMessages(Action<ExecutionDataflowBlockOptions> configure = null);
    }

    public abstract class Endpoint<TSelf> : IEndpoint where TSelf : IEndpoint
    {
        /// <summary>
        /// Descriptive Name for this listener. Optional.
        /// </summary>
        public string Name { get; set; }

        public abstract TSelf Parse(Uri uri);

        public Uri Uri { get; protected set; }

        private bool _isDurable;

        /// <summary>
        /// Should this transport endpoint be wrapped
        /// with persistent, durable messaging?
        /// </summary>
        public bool IsDurable()
        {
            return _isDurable;
        }

        public bool IsListener()
        {
            return _isListening;
        }

        public ExecutionDataflowBlockOptions ExecutionOptions { get; set; } = new ExecutionDataflowBlockOptions();

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
        private bool _isListening;

        public TSelf Subscribe(Subscription subscription)
        {
            _subscriptions.Add(subscription);
            return returnThis();
        }

        IEndpoint IEndpoint.ListenForMessages(Action<ExecutionDataflowBlockOptions> configure = null)
        {
            return ListenForMessages(configure);
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


    }

}
