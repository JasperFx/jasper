using System;
using System.Collections.Generic;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Logging
{
    public interface ITransportLogger
    {
        void OutgoingBatchSucceeded(OutgoingMessageBatch batch);
        void OutgoingBatchFailed(OutgoingMessageBatch batch, Exception ex = null);
        void IncomingBatchReceived(IEnumerable<Envelope> envelopes);


        /// <summary>
        /// The sending agent for this destination experienced too many failures and has been latched
        /// </summary>
        /// <param name="destination"></param>
        void CircuitBroken(Uri destination);

        /// <summary>
        /// The sending agent for this destination has been successfully pinged and un-latched
        /// </summary>
        /// <param name="destination"></param>
        void CircuitResumed(Uri destination);

        /// <summary>
        /// Scheduled jobs were queued for execution
        /// </summary>
        /// <param name="envelopes"></param>
        void ScheduledJobsQueuedForExecution(IEnumerable<Envelope> envelopes);

        /// <summary>
        /// Incoming messages were recovered from storage
        /// </summary>
        /// <param name="envelopes"></param>
        void RecoveredIncoming(IEnumerable<Envelope> envelopes);

        /// <summary>
        /// Outgoing messages were recovered from storage
        /// </summary>
        /// <param name="envelopes"></param>
        void RecoveredOutgoing(IEnumerable<Envelope> envelopes);

        /// <summary>
        /// Outgoing envelopes are discarded because their DeliverBy has expired
        /// </summary>
        /// <param name="envelopes"></param>
        void DiscardedExpired(IEnumerable<Envelope> envelopes);

        /// <summary>
        /// Catch all hook for any exceptions encountered by the messaging
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="correlationId"></param>
        /// <param name="message"></param>
        void LogException(Exception ex, string correlationId = null, string message = "Exception detected:");

        /// <summary>
        /// Logged when the outgoing message recovery finds envelopes with a Destination
        /// that cannot be resolved to a known transport in the system
        /// </summary>
        /// <param name="envelopes"></param>
        void DiscardedUnknownTransport(IEnumerable<Envelope> envelopes);
    }
}
