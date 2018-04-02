using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Tracking;
using Microsoft.Extensions.Logging;
using StoryTeller;
using StoryTeller.Results;
using Envelope = Jasper.Messaging.Runtime.Envelope;

namespace Jasper.Storyteller.Logging
{
    public class StorytellerMessageLogger : IMessageLogger
    {
        private readonly IMessageLogger _inner;
        private ISpecContext _context;
        private readonly List<EnvelopeRecord> _records = new List<EnvelopeRecord>();
        private readonly List<PublisherSubscriberMismatch> _mismatches = new List<PublisherSubscriberMismatch>();

        public StorytellerMessageLogger(IMessageLogger inner)
        {
            _inner = inner;
            Errors = new BusErrors();
        }

        public void Start(ISpecContext context)
        {
            _records.Clear();
            Errors = new BusErrors();
            _mismatches.Clear();

            _context = context;
        }

        public BusErrors Errors { get; private set; }

        public Report[] BuildReports()
        {
            return new Report[]{Errors, new MessageHistoryReport(_records.ToArray())};
        }

        public string ServiceName { get; set; } = "Jasper";

        private void trace(Envelope envelope, string message, Exception ex)
        {
            _records.Add(new EnvelopeRecord(envelope, _context.Timings.Duration, message, ServiceName)
            {
                ExceptionText = ex.ToString()
            });
        }

        private void trace(Envelope envelope, string message)
        {
            if (_context == null) return;
            _records.Add(new EnvelopeRecord(envelope, _context.Timings.Duration, message, ServiceName));
        }

        public void Sent(Envelope envelope)
        {
            trace(envelope, "Sent");
            _inner.Sent(envelope);
        }

        public void Received(Envelope envelope)
        {
            trace(envelope, "Received");
            _inner.Received(envelope);
        }

        public void ExecutionStarted(Envelope envelope)
        {
            trace(envelope, "Execution Started");
            _inner.ExecutionStarted(envelope);
        }

        public void ExecutionFinished(Envelope envelope)
        {
            trace(envelope, "Execution Finished");
            _inner.ExecutionFinished(envelope);
        }

        public void MessageSucceeded(Envelope envelope)
        {
            trace(envelope, "Message Succeeded");
            _inner.MessageSucceeded(envelope);
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            trace(envelope, "Message Failed", ex);
            _inner.MessageFailed(envelope, ex);
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            Errors.Exceptions.Add(ex);
            _inner.LogException(ex, correlationId, message);
        }

        public void NoHandlerFor(Envelope envelope)
        {
            trace(envelope, "No known message handler");
            _inner.NoHandlerFor(envelope);
        }

        public void NoRoutesFor(Envelope envelope)
        {
            trace(envelope, "No message routes");
            _inner.NoRoutesFor(envelope);
        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            _mismatches.Add(mismatch);
            _inner.SubscriptionMismatch(mismatch);
        }

        public void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            trace(envelope, "Was moved to the error queue");
            Errors.Exceptions.Add(ex);

            _inner.MovedToErrorQueue(envelope, ex);
        }

        public void DiscardedEnvelope(Envelope envelope)
        {
            _inner.DiscardedEnvelope(envelope);
        }
    }
}
