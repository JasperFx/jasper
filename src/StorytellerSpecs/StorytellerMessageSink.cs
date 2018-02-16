using System;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Storyteller.Logging;
using StoryTeller;

namespace StorytellerSpecs
{
    public class StorytellerMessageSink : IMessageEventSink
    {
        private readonly ISpecContext _context;

        public StorytellerMessageSink(ISpecContext context)
        {
            _context = context;
        }

        public void Sent(Envelope envelope)
        {
            trace($"Sent {envelope}");
        }

        public void Received(Envelope envelope)
        {
            trace($"Received {envelope}");
        }

        public void ExecutionStarted(Envelope envelope)
        {

        }

        public void ExecutionFinished(Envelope envelope)
        {

        }

        public void MessageSucceeded(Envelope envelope)
        {
            trace($"Message {envelope} succeeded");
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            trace($"Message {envelope} failed");
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            _context.Reporting.ReporterFor<BusErrors>().Exceptions.Add(ex);
        }

        public void NoHandlerFor(Envelope envelope)
        {
            trace($"No handler for {envelope}");
        }

        public void NoRoutesFor(Envelope envelope)
        {
            trace($"No routing for {envelope}");
        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            trace($"Subscription mismatch: {mismatch}");
        }

        public void Undeliverable(Envelope envelope)
        {
            trace($"Envelope {envelope} cannot be delivered");
        }

        public void MovedToErrorQueue(Envelope envelope, Exception ex)
        {

        }

        private void trace(string message)
        {
            _context.Reporting.ReporterFor<BusActivity>().Messages.Add(message);
        }
    }
}
