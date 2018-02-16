using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;

namespace Jasper.Messaging.Logging
{
    public abstract class MessageSinkBase : IMessageEventSink
    {
        public virtual void Sent(Envelope envelope)
        {

        }

        public virtual void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            MessageFailed(envelope, ex);
        }

        public virtual void Received(Envelope envelope)
        {
        }

        public virtual void ExecutionStarted(Envelope envelope)
        {
        }

        public virtual void ExecutionFinished(Envelope envelope)
        {
        }

        public virtual void MessageSucceeded(Envelope envelope)
        {
        }

        public virtual void MessageFailed(Envelope envelope, Exception ex)
        {
            LogException(ex);
        }

        public virtual void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
        }

        public virtual void NoHandlerFor(Envelope envelope)
        {
        }

        public virtual void NoRoutesFor(Envelope envelope)
        {

        }

        public virtual void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {

        }

    }
}
