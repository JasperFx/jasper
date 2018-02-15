using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Routing;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public static class RuntimeSubscriptionExtensions
    {
        public static Task PersistSubscriptions(this JasperRuntime runtime)
        {
            var repository = runtime.Get<ISubscriptionsRepository>();
            return repository.PersistCapabilities(runtime.Capabilities);
        }

        public static void ResetSubscriptions(this JasperRuntime runtime)
        {
            runtime.Get<IMessageRouter>().ClearAll();
        }

    }
}
