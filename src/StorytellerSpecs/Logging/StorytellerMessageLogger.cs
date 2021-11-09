using System;
using System.Collections.Generic;
using Jasper;
using Jasper.Logging;
using Jasper.Tracking;
using Microsoft.Extensions.Logging;
using StoryTeller;
using StoryTeller.Results;
using Envelope = Jasper.Envelope;

namespace StorytellerSpecs.Logging
{
    public class StorytellerMessageLogger : MessageTrackingLogger
    {
        private readonly List<EnvelopeRecord> _records = new List<EnvelopeRecord>();
        private ISpecContext _context;

        public StorytellerMessageLogger(ILoggerFactory factory, IMetrics metrics, JasperOptions options) : base(options,
            factory, metrics)
        {
            Errors = new BusErrors();
        }

        public BusErrors Errors { get; private set; }

        public string ServiceName { get; set; } = "Jasper";


        public void Start(ISpecContext context)
        {
            _records.Clear();
            Errors = new BusErrors();

            _context = context;
        }

        public Report[] BuildReports()
        {
            return new Report[] {Errors, new MessageHistoryReport(_records.ToArray())};
        }

        private void trace(Envelope envelope, string message, Exception ex)
        {
            _records.Add(new EnvelopeRecord(envelope, _context.Timings.Duration, message, ServiceName)
            {
                ExceptionText = ex.ToString()
            });
        }

        private void trace(Envelope envelope, string message)
        {
            if (_context == null)
            {
                return;
            }

            _records.Add(new EnvelopeRecord(envelope, _context.Timings.Duration, message, ServiceName));
        }

        public override void Sent(Envelope envelope)
        {
            trace(envelope, "Sent");
            base.Sent(envelope);
        }

        public override void Received(Envelope envelope)
        {
            trace(envelope, "Received");
            base.Received(envelope);
        }

        public override void ExecutionStarted(Envelope envelope)
        {
            trace(envelope, "Execution Started");
            base.ExecutionStarted(envelope);
        }

        public override void ExecutionFinished(Envelope envelope)
        {
            trace(envelope, "Execution Finished");
            base.ExecutionFinished(envelope);
        }

        public override void MessageSucceeded(Envelope envelope)
        {
            trace(envelope, "Message Succeeded");
            base.MessageSucceeded(envelope);
        }

        public override void MessageFailed(Envelope envelope, Exception ex)
        {
            trace(envelope, "Message Failed", ex);
            base.MessageFailed(envelope, ex);
        }

        public override void LogException(Exception ex, object correlationId = null,
            string message = "Exception detected:")
        {
            Errors.Exceptions.Add(ex);
            base.LogException(ex, correlationId, message);
        }

        public override void NoHandlerFor(Envelope envelope)
        {
            trace(envelope, "No known message handler");
            base.NoHandlerFor(envelope);
        }

        public override void NoRoutesFor(Envelope envelope)
        {
            trace(envelope, "No message routes");
            base.NoRoutesFor(envelope);
        }

        public override void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            trace(envelope, "Was moved to the error queue");
            Errors.Exceptions.Add(ex);

            base.MovedToErrorQueue(envelope, ex);
        }
    }
}
