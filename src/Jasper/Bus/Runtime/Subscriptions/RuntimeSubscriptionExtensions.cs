using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Routing;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public static class RuntimeSubscriptionExtensions
    {
        // TODO -- do a full replacement by service name here.
        public static Task PersistSubscriptions(this JasperRuntime runtime)
        {
            throw new NotImplementedException();
        }

        public static void ResetSubscriptions(this JasperRuntime runtime)
        {
            runtime.Get<IMessageRouter>().ClearAll();
        }

    }
}
