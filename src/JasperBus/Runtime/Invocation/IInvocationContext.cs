using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using JasperBus.Configuration;
using JasperBus.ErrorHandling;
using JasperBus.Model;

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
        Task Retry(Envelope envelope);
    }

    public class EnvelopeContext : IEnvelopeContext
    {
        private readonly HandlerPipeline _pipeline;
        private readonly IList<object> _outgoing = new List<object>();

        public EnvelopeContext(HandlerPipeline pipeline, Envelope envelope)
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
            // TODO -- actually do something here;)
        }

        public void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages)
        {
            // TODO -- actually do something here;)
        }

        public void SendOutgoingMessage(Envelope original, object cascadingMessage)
        {
            // TODO -- actually do something here;)
        }

        public void SendFailureAcknowledgement(Envelope original, string message)
        {
            // TODO -- actually do something here;)
        }

        public void Error(string correlationId, string message, Exception exception)
        {
            // TODO -- actually do something here;)
        }

        public Task Retry(Envelope envelope)
        {
            _outgoing.Clear();

            return _pipeline.ProcessMessage(envelope, this);
        }

        public IContinuation DetermineContinuation(Exception exception, HandlerChain handlerChain, HandlerGraph graph)
        {
            if (Envelope.Attempts >= handlerChain.MaximumAttempts) return new MoveToErrorQueue(exception);

            return handlerChain.DetermineContinuation(Envelope, exception)
                   ?? graph.DetermineContinuation(Envelope, exception)
                   ?? new MoveToErrorQueue(exception);
        }
    }

    // START HERE!!!!!
    // TODO -- this is going to have to return a TASK
}