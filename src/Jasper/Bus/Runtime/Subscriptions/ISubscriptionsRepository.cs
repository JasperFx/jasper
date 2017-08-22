using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Subscriptions.New;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptionsRepository : IDisposable
    {
        Task PersistSubscriptions(IEnumerable<Subscription> subscriptions);
        Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions);

        Task<Subscription[]> GetSubscribersFor(Type messageType);
    }


}
