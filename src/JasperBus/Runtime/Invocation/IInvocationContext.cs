using System;
using System.Collections.Generic;

namespace JasperBus.Runtime.Invocation
{
    /// <summary>
    /// Models a queue of outgoing messages as a result of the current message so you don't even try to
    /// send replies until the original message succeeds
    /// Plus giving you the ability to set the correlation identifiers
    /// </summary>
    public interface IInvocationContext
    {
        /// <summary>
        /// Register a message to be sent via the service bus
        /// as a result of the current message succeeding
        /// </summary>
        /// <param name="message"></param>
        void EnqueueCascading(object message);

        IEnumerable<object> OutgoingMessages();

        Envelope Envelope { get; }

    }

    public interface IEnvelopeContext : IInvocationContext, IDisposable
    {
        void SendAllQueuedOutgoingMessages();

        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);

        void SendOutgoingMessage(Envelope original, object cascadingMessage);

        void SendFailureAcknowledgement(Envelope original, string message);

        void Error(string correlationId, string message, Exception exception);

        // doesn't need to be passed the envelope here, but maybe leave this one
        void Retry(Envelope envelope);
    }

    public class EnvelopeContext : IEnvelopeContext
    {
        private readonly IHandlerPipeline _pipeline;

        public EnvelopeContext(IHandlerPipeline pipeline, Envelope envelope)
        {
            Envelope = envelope;
            _pipeline = pipeline;
        }

        public void EnqueueCascading(object message)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> OutgoingMessages()
        {
            throw new NotImplementedException();
        }

        public Envelope Envelope { get; }

        public void Dispose()
        {
        }

        public void SendAllQueuedOutgoingMessages()
        {
            throw new NotImplementedException();
        }

        public void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages)
        {
            throw new NotImplementedException();
        }

        public void SendOutgoingMessage(Envelope original, object cascadingMessage)
        {
            throw new NotImplementedException();
        }

        public void SendFailureAcknowledgement(Envelope original, string message)
        {
            throw new NotImplementedException();
        }

        public void Error(string correlationId, string message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Retry(Envelope envelope)
        {
            // Call back to the HandlerPipeline with itself to avoid unnecessary work here
            // clear out all queued cascading messages first
            throw new NotImplementedException();
        }
    }

    public interface IContinuation
    {
        void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow);
    }
}