using System;
using System.Collections.Generic;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Logging
{
    public abstract class TransportLoggerBase : ITransportLogger
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

        public virtual void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {

        }
    }
}
