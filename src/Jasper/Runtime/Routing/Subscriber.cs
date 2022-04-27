using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Configuration;
using Jasper.Transports;

namespace Jasper.Runtime.Routing
{
    public abstract class Subscriber : ISubscriber
    {
        private EndpointMode _mode = EndpointMode.BufferedInMemory;

        public IList<Subscription> Subscriptions { get; } = new List<Subscription>();

        /// <summary>
        ///     Mark whether or not the receiver for this listener should use
        ///     message persistence for durability
        /// </summary>
        public EndpointMode Mode
        {
            get => _mode;
            set
            {
                if (!supportsMode(value))
                {
                    throw new InvalidOperationException(
                        $"Endpoint of type {GetType().Name} does not support EndpointMode.{value}");
                }
                _mode = value;
            }
        }

        protected virtual bool supportsMode(EndpointMode mode)
        {
            return true;
        }

        public bool ShouldSendMessage(Type messageType)
        {
            return Subscriptions.Any(x => x.Matches(messageType));
        }

        public abstract void AddRoute(MessageTypeRouting routing, IJasperRuntime runtime);
    }
}
