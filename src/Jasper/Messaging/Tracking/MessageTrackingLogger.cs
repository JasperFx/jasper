using System;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;
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


        public MessageTrackingLogger(MessageHistory history, ILoggerFactory factory) : base(factory)
        {
            _history = history;
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
