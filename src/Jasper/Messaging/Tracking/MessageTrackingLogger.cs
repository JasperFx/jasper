using System;
using System.Collections.Generic;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging.Tracking
{
    /// <summary>
    /// Useful for automated testing scenarios against the service bus to "know"
    /// when all outstanding messages are completed. DO NOT USE IN PRODUCTION!!!
    /// </summary>
    public class MessageTrackingLogger : MessageLogger
    {
        public static readonly string Envelope = "Envelope";
        public static readonly string Execution = "Execution";

        private readonly MessageHistory _history;


        public MessageTrackingLogger(MessageHistory history, ILoggerFactory factory, IMetrics metrics) : base(factory, metrics)
        {
            _history = history;
        }

        public override void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            _history.LogException(ex);
            base.LogException(ex, correlationId, message);
        }


        public override void Sent(Envelope envelope)
        {
            _history.Start(envelope, Envelope);
            base.Sent(envelope);
        }

        public override void ExecutionStarted(Envelope envelope)
        {
            _history.Start(envelope, Execution);
            base.ExecutionStarted(envelope);
        }

        public override void ExecutionFinished(Envelope envelope)
        {
            _history.Complete(envelope, Execution);
            base.ExecutionFinished(envelope);
        }

        public override void MessageSucceeded(Envelope envelope)
        {
            _history.Complete(envelope, Envelope);
            base.MessageSucceeded(envelope);
        }

        public override void MessageFailed(Envelope envelope, Exception ex)
        {
            _history.Complete(envelope, Envelope, ex);
            base.MessageFailed(envelope, ex);
        }
    }


}
