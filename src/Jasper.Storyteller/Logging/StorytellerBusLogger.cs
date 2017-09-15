using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using StoryTeller;
using StoryTeller.Results;
using Envelope = Jasper.Bus.Runtime.Envelope;

namespace Jasper.Storyteller.Logging
{
    public class StorytellerBusLogger : IBusLogger
    {
        private ISpecContext _context;
        private readonly List<EnvelopeRecord> _records = new List<EnvelopeRecord>();
        private readonly List<PublisherSubscriberMismatch> _mismatches = new List<PublisherSubscriberMismatch>();
        private BusErrors _errors;

        public void Start(ISpecContext context)
        {
            _records.Clear();
            _errors = new BusErrors();
            _mismatches.Clear();

            _context = context;
        }

        public Report[] BuildReports()
        {
            return new Report[]{_errors, new MessageHistoryReport(_records.ToArray())};
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

        public void Sent(Envelope envelope)
        {
            trace(envelope, "Sent");
        }

        public void Received(Envelope envelope)
        {
            trace(envelope, "Received");
        }

        public void ExecutionStarted(Envelope envelope)
        {
            trace(envelope, "Execution Started");
        }

        public void ExecutionFinished(Envelope envelope)
        {
            trace(envelope, "Execution Finished");
        }

        public void MessageSucceeded(Envelope envelope)
        {
            trace(envelope, "Message Succeeded");
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            trace(envelope, "Message Failed", ex);
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {
            _errors.Exceptions.Add(ex);
        }

        public void NoHandlerFor(Envelope envelope)
        {
            trace(envelope, "No known message handler");
        }

        public void NoRoutesFor(Envelope envelope)
        {
            trace(envelope, "No message routes");
        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            _mismatches.Add(mismatch);
        }

        public void Undeliverable(Envelope envelope)
        {
            trace(envelope, "Could not be delivered");
        }
    }
}
