using System;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public interface ISubscriptionExpression : ISubscriptionReceiverExpression
    {
        ISubscriptionExpression To<T>();
        ISubscriptionExpression To(Type messageType);
        ISubscriptionExpression To(Func<Type, bool> filter);
    }
}