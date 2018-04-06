using Jasper.Messaging.Runtime.Routing;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public class SubscriptionsHandler
    {
        public void Handle(SubscriptionsChanged message, IMessageRouter router)
        {
            router.ClearAll();
        }
    }


    public class SubscriptionsChanged
    {
    }
}
