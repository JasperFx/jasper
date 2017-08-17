namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public interface ISubscriptions : ISubscriptionExpression
    {
        ISubscriptionReceiverExpression ToAllMessages();
    }
}