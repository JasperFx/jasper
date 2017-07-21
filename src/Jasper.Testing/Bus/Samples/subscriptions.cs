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

            // Local subscription to only this node
            SubscribeLocally()
                .ToSource(settings.OtherApp)
                .ToMessage<OtherAppMessage1>();

            // Global subscription to the all the
            // running nodes in this clustered application
            SubscribeAt(settings.Receiving)
                .ToSource(settings.OtherApp)
                .ToMessage<OtherAppMessage2>()
                .ToMessage<OtherAppMessage3>();
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
        public Task PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            throw new NotImplementedException();
        }

        public Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            throw new NotImplementedException();
        }

        public Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            throw new NotImplementedException();
        }

        Task<Subscription[]> ISubscriptionsRepository.GetSubscribersFor(Type messageType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Uri> GetSubscribersFor(Type messageType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Subscription> ActiveSubscriptions { get; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void RegisterForChanges(Action<IEnumerable<Subscription>> updateHandler)
        {
            throw new NotImplementedException();
        }
    }
}
