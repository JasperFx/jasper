using System;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public interface ISubscriptionReceiverExpression
    {
        void At(Uri receiver);
        void At(string receiverUriString);
    }
}