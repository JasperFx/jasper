using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Bus.Logging
{

// TODO -- rename to IMessageLogger?

    // SAMPLE: IBusLogger
    public interface IBusLogger
    {
        /// <summary>
        /// Called when an envelope is successfully sent through a transport
        /// </summary>
        /// <param name="envelope"></param>
        void Sent(Envelope envelope);

        /// <summary>
        /// Called when an envelope is first received by the current application
        /// </summary>
        /// <param name="envelope"></param>
        void Received(Envelope envelope);

        /// <summary>
        /// Marks the beginning of message execution
        /// </summary>
        /// <param name="envelope"></param>
        void ExecutionStarted(Envelope envelope);

        /// <summary>
        /// Marks the end of message execution
        /// </summary>
        /// <param name="envelope"></param>
        void ExecutionFinished(Envelope envelope);

        /// <summary>
        /// Called when a message has been successfully processed
        /// </summary>
        /// <param name="envelope"></param>
        void MessageSucceeded(Envelope envelope);

        /// <summary>
        /// Called when message execution has failed
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="ex"></param>
        void MessageFailed(Envelope envelope, Exception ex);

        /// <summary>
        /// Catch all hook for any exceptions encountered by the messaging
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="correlationId"></param>
        /// <param name="message"></param>
        void LogException(Exception ex, string correlationId = null, string message = "Exception detected:");

        /// <summary>
        /// Called when a message is received for which the application has no handler
        /// </summary>
        /// <param name="envelope"></param>
        void NoHandlerFor(Envelope envelope);

        /// <summary>
        /// Called when a Jasper application tries to send a message but cannot determine
        /// any subscribers or matching publishing rules
        /// </summary>
        /// <param name="envelope"></param>
        void NoRoutesFor(Envelope envelope);

        /// <summary>
        /// Called when a Jasper application cannot send a message to a known subscriber because of
        /// mismatches in either message version or representation or supported transports
        /// </summary>
        /// <param name="mismatch"></param>
        void SubscriptionMismatch(PublisherSubscriberMismatch mismatch);

        /// <summary>
        /// Called when Jasper moves an envelope into the dead letter queue
        /// </summary>
        /// <param name="envelope"></param>
        void MovedToErrorQueue(Envelope envelope, Exception ex);
    }
    // ENDSAMPLE
}
