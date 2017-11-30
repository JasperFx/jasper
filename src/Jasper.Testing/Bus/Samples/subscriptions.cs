using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime.Subscriptions;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Bus.Samples
{
    public class OtherAppMessage1 { }
    public class OtherAppMessage2 { }
    public class OtherAppMessage3 { }

    // SAMPLE: NodeSettings
    public class NodeSettings
    {
        // This uri points to a different
        // application
        public Uri OtherApp { get; set; }

        // This uri should be the shared
        // channel that all nodes in the
        // application cluster are reading
        public Uri Receiving { get; set; }
    }
    // ENDSAMPLE

    // SAMPLE: configuring-subscriptions
    public class LocalApp : JasperRegistry
    {
        public LocalApp()
        {
            // Explicitly set the logical descriptive
            // name of this application. The default is
            // derived from the name of the class
            ServiceName = "MyApplication";

            // Incoming messages
            Transports.ListenForMessagesFrom("tcp://localhost:2333");

            // *Optionally* make the subscriptions to the location of the load
            // balancer in front of your logical application nodes
            Subscribe.At("tcp://loadbalancer:2333");

            // Declare subscriptions to specific message types
            Subscribe
                .To<OtherAppMessage1>()
                .To<OtherAppMessage2>()
                .To<OtherAppMessage3>();

            // Or just quickly say, "send me everything that
            // I understand how to handle"
            Subscribe.ToAllMessages();
        }
    }
    // ENDSAMPLE

    // SAMPLE: SubscriptionStorageOverride
    public class SubscriptionStorageApp : JasperRegistry
    {
        public SubscriptionStorageApp()
        {
            // Plug in subscription storage backed by Marten
            Services.AddSingleton<ISubscriptionsRepository, MartenSubscriptionRepository>();
        }
    }
    // ENDSAMPLE

    public class MartenSubscriptionRepository : ISubscriptionsRepository
    {
        public void Dispose()
        {
            throw new NotSupportedException();
        }

        public Task PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            throw new NotSupportedException();
        }

        public Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            throw new NotSupportedException();
        }

        public Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            throw new NotSupportedException();
        }

        public Task<Subscription[]> GetSubscriptions()
        {
            throw new NotImplementedException();
        }

        public Task ReplaceSubscriptions(string serviceName, Subscription[] subscriptions)
        {
            throw new NotSupportedException();
        }
    }
}
