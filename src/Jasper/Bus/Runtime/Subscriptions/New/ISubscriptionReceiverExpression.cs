using System;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public interface ISubscriptionReceiverExpression
    {
        void At(Uri receiver);
        void At(string receiverUriString);
    }
}