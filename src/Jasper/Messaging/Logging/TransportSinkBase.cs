using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Logging
{
    public abstract class TransportSinkBase : ITransportEventSink
    {
        public virtual void OutgoingBatchSucceeded(OutgoingMessageBatch batch)
        {
        }

        public virtual void OutgoingBatchFailed(OutgoingMessageBatch batch, Exception ex = null)
        {
            LogException(ex);
        }

        public virtual void IncomingBatchReceived(IEnumerable<Envelope> envelopes)
        {
        }

        public virtual void CircuitBroken(Uri destination)
        {
        }

        public virtual void CircuitResumed(Uri destination)
        {
        }

        public virtual void ScheduledJobsQueuedForExecution(IEnumerable<Envelope> envelopes)
        {
        }

        public virtual void RecoveredIncoming(IEnumerable<Envelope> envelopes)
        {
        }

        public virtual void RecoveredOutgoing(IEnumerable<Envelope> envelopes)
        {
        }

        public virtual void DiscardedExpired(IEnumerable<Envelope> envelopes)
        {

        }

        public virtual void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {

        }

        public virtual void DiscardedUnknownTransport(IEnumerable<Envelope> envelopes)
        {
        }
    }
}
