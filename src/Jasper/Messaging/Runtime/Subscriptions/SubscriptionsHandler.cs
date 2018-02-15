using Jasper.Messaging.Runtime.Routing;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public class SubscriptionsHandler
    {
        private readonly IMessageRouter _router;

        public SubscriptionsHandler(IMessageRouter router)
        {
            _router = router;
        }

        public void Handle(SubscriptionsChanged message)
        {
            _router.ClearAll();
        }
    }


    public class SubscriptionsChanged
    {
    }
}
