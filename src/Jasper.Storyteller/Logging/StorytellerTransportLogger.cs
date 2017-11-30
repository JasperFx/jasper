using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Logging;
using Jasper.Bus.Transports.Tcp;
using StoryTeller;
using StoryTeller.Results;
using StoryTeller.Util;
using Envelope = Jasper.Bus.Runtime.Envelope;

namespace Jasper.Storyteller.Logging
{
    public class StorytellerTransportLogger : ITransportLogger
    {
        private ISpecContext _context;
        private TransportLoggingReport _report;
        private BusErrors _errors;

        public void Start(ISpecContext context, BusErrors errors)
        {
            _errors = errors;
            _report = new TransportLoggingReport();
            _context = context;

            _context.Reporting.Log(_report);
        }

        public void OutgoingBatchSucceeded(OutgoingMessageBatch batch)
        {
            _report.Trace($"Successfully sent {batch.Messages.Count} messages to {batch.Destination}");
        }

        public void IncomingBatchReceived(IEnumerable<Envelope> envelopes)
        {
            _report.Trace($"Successfully received {envelopes.Count()} messages");
        }

        public void OutgoingBatchFailed(OutgoingMessageBatch batch, Exception ex = null)
        {
            LogException(ex);
        }

        public void CircuitBroken(Uri destination)
        {
            _report.Trace($"Sending agent for {destination} is latched");
        }

        public void CircuitResumed(Uri destination)
        {
            _report.Trace($"Sending agent for {destination} has resumed");
        }

        public void ScheduledJobsQueuedForExecution(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                _report.Trace($"Enqueued scheduled job {envelope} locally");
            }
        }

        public void RecoveredIncoming(IEnumerable<Envelope> envelopes)
        {
            _report.Trace($"Recovered {envelopes.Count()} incoming envelopes from storage");
        }

        public void RecoveredOutgoing(IEnumerable<Envelope> envelopes)
        {
            _report.Trace($"Recovered {envelopes.Count()} outgoing envelopes from storage");
        }

        public void DiscardedExpired(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                _report.Trace($"Discarded expired envelope {envelope}");
            }
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {
            _errors.Exceptions.Add(ex);
        }

        public void DiscardedUnknownTransport(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                _report.Trace($"Discarded {envelope} with an unknown transport");
            }
        }
    }

    public class TransportLoggingReport : Report
    {
        private readonly IList<HtmlTag> _tags = new List<HtmlTag>();


        public string ToHtml()
        {
            var div = new HtmlTag("ol");

            foreach (var htmlTag in _tags.ToArray())
            {
                div.Append(htmlTag);
            }

            return div.ToString();
        }

        public void Trace(string text)
        {
            _tags.Add(new HtmlTag("li").Text(text));
        }

        public string Title => "Transport Activitry";

        public string ShortTitle => "Transports";

        public int Count => _tags.Count;
    }
}
