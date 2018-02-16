using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Subscriptions;
using StoryTeller;
using StoryTeller.Results;
using Envelope = Jasper.Messaging.Runtime.Envelope;

namespace Jasper.Storyteller.Logging
{
    public class StorytellerMessageSink : MessageSinkBase
    {
        private ISpecContext _context;
        private readonly List<EnvelopeRecord> _records = new List<EnvelopeRecord>();
        private readonly List<PublisherSubscriberMismatch> _mismatches = new List<PublisherSubscriberMismatch>();

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
            _records.Add(new EnvelopeRecord(envelope, _context.Timings.Duration, message, ServiceName));
        }

        public override void Sent(Envelope envelope)
        {
            trace(envelope, "Sent");
        }

        public override void Received(Envelope envelope)
        {
            trace(envelope, "Received");
        }

        public override void ExecutionStarted(Envelope envelope)
        {
            trace(envelope, "Execution Started");
        }

        public override void ExecutionFinished(Envelope envelope)
        {
            trace(envelope, "Execution Finished");
        }

        public override void MessageSucceeded(Envelope envelope)
        {
            trace(envelope, "Message Succeeded");
        }

        public override void MessageFailed(Envelope envelope, Exception ex)
        {
            trace(envelope, "Message Failed", ex);
        }

        public override void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            Errors.Exceptions.Add(ex);
        }

        public override void NoHandlerFor(Envelope envelope)
        {
            trace(envelope, "No known message handler");
        }

        public override void NoRoutesFor(Envelope envelope)
        {
            trace(envelope, "No message routes");
        }

        public override void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            _mismatches.Add(mismatch);
        }

        public override void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            trace(envelope, "Was moved to the error queue");
            Errors.Exceptions.Add(ex);
        }
    }
}
