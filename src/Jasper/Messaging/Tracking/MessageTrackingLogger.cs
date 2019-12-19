using System;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging.Tracking
{
    /// <summary>
    ///     Useful for automated testing scenarios against the service bus to "know"
    ///     when all outstanding messages are completed. DO NOT USE IN PRODUCTION!!!
    /// </summary>
    public class MessageTrackingLogger : MessageLogger
    {
        private string _serviceName;
        private int _uniqueNodeId;


        public MessageTrackingLogger(JasperOptions options, ILoggerFactory factory, IMetrics metrics) : base(factory,
            metrics)
        {
            _uniqueNodeId = options.Advanced.UniqueNodeId;
            _serviceName = options.ServiceName;
        }

        public TrackedSession ActiveSession { get; internal set; }

        

        public override void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            ActiveSession?.Record(EventType.MovedToErrorQueue, envelope, _serviceName, _uniqueNodeId);
            base.MovedToErrorQueue(envelope, ex);
        }

        public override void NoHandlerFor(Envelope envelope)
        {
            ActiveSession?.Record(EventType.NoHandlers, envelope, _serviceName, _uniqueNodeId);
            base.NoHandlerFor(envelope);
        }

        public override void NoRoutesFor(Envelope envelope)
        {
            ActiveSession?.Record(EventType.NoRoutes, envelope, _serviceName, _uniqueNodeId);
            base.NoHandlerFor(envelope);
        }

        public override void LogException(Exception ex, Guid correlationId = default(Guid),
            string message = "Exception detected:")
        {
            ActiveSession?.LogException(ex, _serviceName);
            base.LogException(ex, correlationId, message);
        }

        public override void Received(Envelope envelope)
        {
            ActiveSession?.Record(EventType.Received, envelope, _serviceName, _uniqueNodeId);
            base.Received(envelope);
        }

        public override void Sent(Envelope envelope)
        {
            ActiveSession?.Record(EventType.Sent, envelope, _serviceName, _uniqueNodeId);
            base.Sent(envelope);
        }

        public override void ExecutionStarted(Envelope envelope)
        {
            ActiveSession?.Record(EventType.ExecutionStarted, envelope, _serviceName, _uniqueNodeId);
            base.ExecutionStarted(envelope);
        }

        public override void ExecutionFinished(Envelope envelope)
        {
            ActiveSession?.Record(EventType.ExecutionFinished, envelope, _serviceName, _uniqueNodeId);
            base.ExecutionFinished(envelope);
        }

        public override void MessageSucceeded(Envelope envelope)
        {
            ActiveSession?.Record(EventType.MessageSucceeded, envelope, _serviceName, _uniqueNodeId);
            base.MessageSucceeded(envelope);
        }

        public override void MessageFailed(Envelope envelope, Exception ex)
        {
            ActiveSession?.Record(EventType.Sent, envelope, _serviceName, _uniqueNodeId, ex);
            base.MessageFailed(envelope, ex);
        }
    }
}
