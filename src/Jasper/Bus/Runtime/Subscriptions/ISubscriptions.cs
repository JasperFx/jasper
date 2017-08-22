namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptions : ISubscriptionExpression
    {
        ISubscriptionReceiverExpression ToAllMessages();
    }
}