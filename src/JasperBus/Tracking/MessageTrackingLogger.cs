using System;
using JasperBus.Runtime;

namespace JasperBus.Tracking
{
    /// <summary>
    /// Useful for automated testing scenarios against the service bus to "know"
    /// when all outstanding messages are completed. DO NOT USE IN PRODUCTION!!!
    /// </summary>
    public class MessageTrackingLogger : BusLogger
    {
        public static readonly string Envelope = "Envelope";
        public static readonly string Execution = "Execution";

        private readonly MessageHistory _history;


        public MessageTrackingLogger(MessageHistory history)
        {
            _history = history;
        }

        public override void Sent(Envelope envelope)
        {
            _history.Start(envelope, Envelope);
        }

        public override void ExecutionStarted(Envelope envelope)
        {
            _history.Start(envelope, Execution);
        }

        public override void ExecutionFinished(Envelope envelope)
        {
            _history.Complete(envelope, Execution);
        }

        public override void MessageSucceeded(Envelope envelope)
        {
            _history.Complete(envelope, Envelope);
        }

        public override void MessageFailed(Envelope envelope, Exception ex)
        {
            _history.Complete(envelope, Envelope, ex);
        }
    }

}