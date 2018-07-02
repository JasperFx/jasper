using Jasper.Messaging.Runtime.Subscriptions;
using Lamar.IoC;
using System.Threading.Tasks;

namespace Jasper.Internal.Resolvers
{
    // START: Jasper_Messaging_Runtime_Subscriptions_ISubscriptionsRepository_defaultSubscriptionsRepository
    public class Jasper_Messaging_Runtime_Subscriptions_ISubscriptionsRepository_defaultSubscriptionsRepository : Lamar.IoC.Resolvers.SingletonResolver<Jasper.Messaging.Runtime.Subscriptions.ISubscriptionsRepository>
    {
        private readonly Jasper.Messaging.Runtime.Subscriptions.SubscriptionSettings _subscriptionSettings;
        private readonly Lamar.IoC.Scope _topLevelScope;

        public Jasper_Messaging_Runtime_Subscriptions_ISubscriptionsRepository_defaultSubscriptionsRepository(Jasper.Messaging.Runtime.Subscriptions.SubscriptionSettings subscriptionSettings, Lamar.IoC.Scope topLevelScope) : base(topLevelScope)
        {
            _subscriptionSettings = subscriptionSettings;
            _topLevelScope = topLevelScope;
        }



        public override Jasper.Messaging.Runtime.Subscriptions.ISubscriptionsRepository Build(Lamar.IoC.Scope scope)
        {
            return new Jasper.Messaging.Runtime.Subscriptions.DefaultSubscriptionsRepository(_subscriptionSettings);
        }

    }

    // END: Jasper_Messaging_Runtime_Subscriptions_ISubscriptionsRepository_defaultSubscriptionsRepository
    
    
}

