using System;
using System.Collections.Generic;
using System.Diagnostics;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Logging
{
    public class CompositeTransportLogger : ITransportLogger
    {
        private readonly IExceptionSink[] _exceptions;
        public ITransportEventSink[] Sinks { get; }

        public CompositeTransportLogger(ITransportEventSink[] sinks, IExceptionSink[] exceptions)
        {
            _exceptions = exceptions;
            Sinks = sinks;
        }

        public void OutgoingBatchSucceeded(OutgoingMessageBatch batch)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.OutgoingBatchSucceeded(batch);
                }
                catch (Exception)
                {

                }
            }
        }

        public void OutgoingBatchFailed(OutgoingMessageBatch batch, Exception ex = null)
        {
            try
            {
                ex = ex.Demystify();
            }
            catch (Exception)
            {

            }

            foreach (var logger in Sinks)
            {
                try
                {
                    logger.OutgoingBatchFailed(batch, ex);
                }
                catch (Exception)
                {

                }
            }
        }

        public void IncomingBatchReceived(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.IncomingBatchReceived(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void CircuitBroken(Uri destination)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.CircuitBroken(destination);
                }
                catch (Exception)
                {

                }
            }
        }

        public void CircuitResumed(Uri destination)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.CircuitResumed(destination);
                }
                catch (Exception)
                {

                }
            }
        }

        public void ScheduledJobsQueuedForExecution(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.ScheduledJobsQueuedForExecution(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void RecoveredIncoming(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.RecoveredIncoming(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void RecoveredOutgoing(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.RecoveredOutgoing(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void DiscardedExpired(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.DiscardedExpired(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            try
            {
                ex = ex.Demystify();
            }
            catch (Exception)
            {

            }

            foreach (var logger in _exceptions)
            {
                try
                {
                    logger.LogException(ex, correlationId, message);
                }
                catch (Exception)
                {

                }
            }
        }

        public static CompositeTransportLogger Empty()
        {
            return new CompositeTransportLogger(new ITransportEventSink[0], new IExceptionSink[0]);
        }

        public void DiscardedUnknownTransport(IEnumerable<Envelope> envelopes)
        {
            foreach (var logger in Sinks)
            {
                try
                {
                    logger.DiscardedUnknownTransport(envelopes);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
