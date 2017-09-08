using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime.Subscriptions;

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
        public LocalApp(NodeSettings settings)
        {
            // Explicitly set the logical descriptive
            // name of this application. The default is
            // derived from the name of the class
            ServiceName = "MyApplication";

            // Incoming messages
            Channels.ListenForMessagesFrom(settings.Receiving);

//            // Local subscription to only this node
//            SubscribeLocally()
//                .ToSource(settings.OtherApp)
//                .ToMessage<OtherAppMessage1>();
//
//            // Global subscription to the all the
//            // running nodes in this clustered application
//            SubscribeAt(settings.Receiving)
//                .ToSource(settings.OtherApp)
//                .ToMessage<OtherAppMessage2>()
//                .ToMessage<OtherAppMessage3>();
        }
    }
    // ENDSAMPLE

    // SAMPLE: SubscriptionStorageOverride
    public class SubscriptionStorageApp : JasperRegistry
    {
        public SubscriptionStorageApp()
        {
            // Plug in subscription storage backed by Marten
            Services.ReplaceService<ISubscriptionsRepository, MartenSubscriptionRepository>();
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

        public Task ReplaceSubscriptions(string serviceName, Subscription[] subscriptions)
        {
            throw new NotSupportedException();
        }
    }
}
