using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Bus.Logging
{
    public abstract class BusLoggerBase : IBusLogger
    {
        public virtual void Sent(Envelope envelope)
        {

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
        }

        public virtual void LogException(Exception ex, string correlationId = null, string message = null)
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

        public virtual void Undeliverable(Envelope envelope)
        {
        }
    }
}
