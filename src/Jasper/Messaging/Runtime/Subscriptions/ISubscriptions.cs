namespace Jasper.Messaging.Runtime.Subscriptions
{
    public interface ISubscriptions : ISubscriptionExpression
    {
        ISubscriptionReceiverExpression ToAllMessages();
    }
}