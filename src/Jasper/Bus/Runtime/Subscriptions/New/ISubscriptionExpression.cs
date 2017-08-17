using System;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public interface ISubscriptionExpression : ISubscriptionReceiverExpression
    {
        ISubscriptionExpression To<T>();
        ISubscriptionExpression To(Type messageType);
        ISubscriptionExpression To(Func<Type, bool> filter);
    }
}