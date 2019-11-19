using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging
{
    public class Subscriber : IDisposable
    {
        public Subscriber(ISendingAgent agent, IEnumerable<Subscription> subscriptions)
        {
            Subscriptions.AddRange(subscriptions);
            Agent = agent;
        }

        public ISendingAgent Agent { get; }

        public bool Latched => Agent.Latched;
        public Uri Destination => Agent.Destination;

        public IList<Subscription> Subscriptions { get; } = new List<Subscription>();

        public bool ShouldSendMessage(Type messageType)
        {
            return Subscriptions.Any(x => x.Matches(messageType));
        }

        public void Dispose()
        {
            Agent?.Dispose();
        }


    }
}
