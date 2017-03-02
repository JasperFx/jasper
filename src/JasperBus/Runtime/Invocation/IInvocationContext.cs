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

    public interface IEnvelopeContext : IDisposable
    {
        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);

        void SendOutgoingMessage(Envelope original, object cascadingMessage);

        void SendFailureAcknowledgement(Envelope original, string message);

        void Error(string correlationId, string message, Exception exception);
        void Retry(Envelope envelope);
    }

    public interface IContinuation
    {
        void Execute(Envelope envelope, IEnvelopeContext context, DateTime utcNow);
    }
}