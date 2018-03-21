using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging.Logging
{
    public class MessageLogger : IMessageLogger
    {
        public const int SentEventId = 100;
        public const int ReceivedEventId = 101;
        public const int ExecutionStartedEventId = 102;
        public const int ExecutionFinishedEventId = 103;
        public const int MessageSucceededEventId = 104;
        public const int MessageFailedEventId = 105;
        public const int NoHandlerEventId = 106;
        public const int NoRoutesEventId = 107;
        public const int SubscriptionMismatchId = 108;
        public const int MovedToErrorQueueId = 108;
        public const int UndeliverableEventId = 108;

        private readonly ILogger _logger;
        private readonly Action<ILogger, string, Guid, Uri, Exception> _sent;
        private readonly Action<ILogger, string, Guid, Uri, Uri, Exception> _received;
        private readonly Action<ILogger, string, Guid, Exception> _executionStarted;
        private readonly Action<ILogger, string, Guid, Exception> _executionFinished;
        private readonly Action<ILogger, string, Guid, Uri, Exception> _messageSucceeded;
        private readonly Action<ILogger, string, Guid, Uri, Exception> _messageFailed;
        private readonly Action<ILogger, string, Guid, Uri, Exception> _noHandler;
        private readonly Action<ILogger, Envelope, Exception> _noRoutes;
        private readonly Action<ILogger, PublisherSubscriberMismatch, Exception> _subscriptionMismatch;
        private readonly Action<ILogger, Envelope, Exception> _movedToErrorQueue;
        private readonly Action<ILogger, Envelope, Exception> _undeliverable;

        public MessageLogger(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger("Jasper.Messages");

            _sent = LoggerMessage.Define<string, Guid, Uri>(LogLevel.Debug, SentEventId,
                "Sent {Name}#{Id} to {Destination}");

            _received = LoggerMessage.Define<string, Guid, Uri, Uri>(LogLevel.Debug, ReceivedEventId,
                "Received {Name}#{Id} at {Destination} from {ReplyUri}");

            _executionStarted = LoggerMessage.Define<string, Guid>(LogLevel.Debug, ExecutionStartedEventId,
                "Started processing {Name}#{Id}");

            _executionFinished = LoggerMessage.Define<string, Guid>(LogLevel.Debug, ExecutionFinishedEventId,
                "Finished processing {Name}#{Id}");

            _messageSucceeded =
                LoggerMessage.Define<string, Guid, Uri>(LogLevel.Information, MessageSucceededEventId,
                    "Successfully processed message {Name}#{envelope} from {ReplyUri}");

            _messageFailed = LoggerMessage.Define<string, Guid, Uri>(LogLevel.Error, MessageFailedEventId,
                "Failed to process message {Name}#{envelope} from {ReplyUri}");

            _noHandler = LoggerMessage.Define<string, Guid, Uri>(LogLevel.Information, NoHandlerEventId,
                "No known handler for {Name}#{Id} from {ReplyUri}");

            _noRoutes = LoggerMessage.Define<Envelope>(LogLevel.Information, NoRoutesEventId,
                "No routes can be determined for {envelope}");

            _subscriptionMismatch =
                LoggerMessage.Define<PublisherSubscriberMismatch>(LogLevel.Error, SubscriptionMismatchId,
                    "Subscriber mismatch: {mismatch}");

            _movedToErrorQueue = LoggerMessage.Define<Envelope>(LogLevel.Error, MovedToErrorQueueId,
                "Envelope {envelope} was moved to the error queue");

            _undeliverable = LoggerMessage.Define<Envelope>(LogLevel.Information, UndeliverableEventId,
                "Could not deliver {envelope}");
        }

        public virtual void Sent(Envelope envelope)
        {
            _sent(_logger, envelope.GetMessageTypeName(), envelope.Id, envelope.Destination, null);
        }

        public virtual void Received(Envelope envelope)
        {
            _received(_logger, envelope.GetMessageTypeName(), envelope.Id, envelope.Destination,
                envelope.ReplyUri, null);
        }

        public virtual void ExecutionStarted(Envelope envelope)
        {
            _executionStarted(_logger, envelope.GetMessageTypeName(), envelope.Id, null);
        }

        public virtual void ExecutionFinished(Envelope envelope)
        {
            _executionFinished(_logger, envelope.GetMessageTypeName(), envelope.Id, null);
        }

        public virtual void MessageSucceeded(Envelope envelope)
        {
            _messageSucceeded(_logger, envelope.GetMessageTypeName(), envelope.Id, envelope.ReplyUri, null);
        }

        public virtual void MessageFailed(Envelope envelope, Exception ex)
        {
            _messageFailed(_logger, envelope.GetMessageTypeName(), envelope.Id, envelope.ReplyUri, ex);
        }

        public virtual void NoHandlerFor(Envelope envelope)
        {
            _noHandler(_logger, envelope.GetMessageTypeName(), envelope.Id, envelope.ReplyUri, null);
        }

        public virtual void NoRoutesFor(Envelope envelope)
        {
            _noRoutes(_logger, envelope, null);
        }

        public virtual void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {
            _subscriptionMismatch(_logger, mismatch, null);
        }

        public virtual void MovedToErrorQueue(Envelope envelope, Exception ex)
        {
            _movedToErrorQueue(_logger, envelope, ex);
        }

        public virtual void DiscardedEnvelope(Envelope envelope)
        {
            _undeliverable(_logger, envelope, null);
        }

        public virtual void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
            _logger.LogError(ex, message);
        }

    }
}
