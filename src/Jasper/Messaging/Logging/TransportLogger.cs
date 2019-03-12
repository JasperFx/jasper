using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Tcp;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging.Logging
{
    public class TransportLogger : ITransportLogger
    {
        public const int OutgoingBatchSucceededEventId = 200;
        public const int OutgoingBatchFailedEventId = 201;
        public const int IncomingBatchReceivedEventId = 202;
        public const int CircuitBrokenEventId = 203;
        public const int CircuitBrokenResumedId = 204;
        public const int ScheduledJobsQueuedForExecutionEventId = 205;
        public const int RecoveredIncomingEventId = 206;
        public const int RecoveredOutgoingEventId = 207;
        public const int DiscardedExpiredEventId = 208;
        public const int DiscardedUnknownTransportEventId = 209;
        public const int ListeningStatusChangedEventId = 210;
        private readonly Action<ILogger, Uri, Exception> _circuitBroken;
        private readonly Action<ILogger, Uri, Exception> _circuitResumed;
        private readonly Action<ILogger, Envelope, Exception> _discardedExpired;
        private readonly Action<ILogger, Envelope, Exception> _discardedUnknownTransport;
        private readonly Action<ILogger, int, Uri, Exception> _incomingBatchReceived;
        private readonly Action<ILogger, ListeningStatus, Exception> _listeningStatusChanged;

        private readonly ILogger _logger;
        private readonly IMetrics _metrics;
        private readonly Action<ILogger, Uri, Exception> _outgoingBatchFailed;
        private readonly Action<ILogger, int, Uri, Exception> _outgoingBatchSucceeded;
        private readonly Action<ILogger, int, Exception> _recoveredIncoming;
        private readonly Action<ILogger, int, Exception> _recoveredOutgoing;
        private readonly Action<ILogger, Envelope, DateTimeOffset, Exception> _scheduledJobsQueued;


        public TransportLogger(ILoggerFactory factory, IMetrics metrics)
        {
            _metrics = metrics;
            _logger = factory.CreateLogger("Jasper.Transports");

            _outgoingBatchSucceeded = LoggerMessage.Define<int, Uri>(LogLevel.Debug, OutgoingBatchSucceededEventId,
                "Successfully sent {Count} messages to {Destination}");

            _outgoingBatchFailed = LoggerMessage.Define<Uri>(LogLevel.Error, OutgoingBatchFailedEventId,
                "Failed to send outgoing envelopes batch to {Destination}");

            _incomingBatchReceived = LoggerMessage.Define<int, Uri>(LogLevel.Debug, IncomingBatchReceivedEventId,
                "Received {Count} message(s) from {ReplyUri}");

            _circuitBroken = LoggerMessage.Define<Uri>(LogLevel.Error, CircuitBrokenEventId,
                "Sending agent for {destination} is latched");

            _circuitResumed = LoggerMessage.Define<Uri>(LogLevel.Information, CircuitBrokenResumedId,
                "Sending agent for {destination} has resumed");

            _scheduledJobsQueued =
                LoggerMessage.Define<Envelope, DateTimeOffset>(LogLevel.Information,
                    ScheduledJobsQueuedForExecutionEventId,
                    "Envelope {envelope} was scheduled locally for {date}");

            _recoveredIncoming = LoggerMessage.Define<int>(LogLevel.Information, RecoveredIncomingEventId,
                "Recovered {Count} incoming envelopes from storage");

            _recoveredOutgoing = LoggerMessage.Define<int>(LogLevel.Information, RecoveredOutgoingEventId,
                "Recovered {Count} outgoing envelopes from storage");

            _discardedExpired = LoggerMessage.Define<Envelope>(LogLevel.Debug, DiscardedExpiredEventId,
                "Discarded expired envelope {envelope}");

            _discardedUnknownTransport =
                LoggerMessage.Define<Envelope>(LogLevel.Information, DiscardedUnknownTransportEventId,
                    "Discarded {envelope} with unknown transport");

            _listeningStatusChanged = LoggerMessage.Define<ListeningStatus>(LogLevel.Information,
                ListeningStatusChangedEventId, "ListeningStatus changed to {status}");
        }

        public virtual void OutgoingBatchSucceeded(OutgoingMessageBatch batch)
        {
            _outgoingBatchSucceeded(_logger, batch.Messages.Count, batch.Destination, null);
        }

        public virtual void OutgoingBatchFailed(OutgoingMessageBatch batch, Exception ex = null)
        {
            _outgoingBatchFailed(_logger, batch.Destination, ex);
        }

        public virtual void IncomingBatchReceived(IEnumerable<Envelope> envelopes)
        {
            _metrics.MessagesReceived(envelopes);

            _incomingBatchReceived(_logger, envelopes.Count(), envelopes.FirstOrDefault()?.ReplyUri, null);
        }

        public virtual void CircuitBroken(Uri destination)
        {
            _metrics.CircuitBroken(destination);
            _circuitBroken(_logger, destination, null);
        }

        public virtual void CircuitResumed(Uri destination)
        {
            _metrics.CircuitResumed(destination);
            _circuitResumed(_logger, destination, null);
        }

        public virtual void ScheduledJobsQueuedForExecution(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
                _scheduledJobsQueued(_logger, envelope, envelope.ExecutionTime.Value, null);
        }

        public virtual void RecoveredIncoming(IEnumerable<Envelope> envelopes)
        {
            _recoveredIncoming(_logger, envelopes.Count(), null);
        }

        public virtual void RecoveredOutgoing(IEnumerable<Envelope> envelopes)
        {
            _recoveredOutgoing(_logger, envelopes.Count(), null);
        }

        public virtual void DiscardedExpired(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes) _discardedExpired(_logger, envelope, null);
        }

        public virtual void DiscardedUnknownTransport(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes) _discardedUnknownTransport(_logger, envelope, null);
        }

        public void ListeningStatusChange(ListeningStatus status)
        {
            _listeningStatusChanged(_logger, status, null);
        }

        public virtual void LogException(Exception ex, Guid correlationId = default(Guid),
            string message = "Exception detected:")
        {
            _metrics.LogException(ex);
            var exMessage = correlationId == Guid.Empty ? message : message + correlationId;
            _logger.LogError(ex, exMessage);
        }

        public static ITransportLogger Empty()
        {
            return new TransportLogger(new LoggerFactory(), new NulloMetrics());
        }
    }
}
