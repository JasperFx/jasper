using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JasperBus.ErrorHandling;
using JasperBus.Model;

namespace JasperBus.Runtime.Invocation
{
    public class EnvelopeContext : IEnvelopeContext
    {
        private readonly HandlerPipeline _pipeline;
        private readonly List<object> _outgoing = new List<object>();

        public EnvelopeContext(HandlerPipeline pipeline, Envelope envelope)
        {
            Envelope = envelope;
            _pipeline = pipeline;
        }

        public IBusLogger Logger => _pipeline.Logger;

        public void EnqueueCascading(object message)
        {
            if (message == null) return;

            var enumerable = message as IEnumerable<object>;
            if (enumerable == null)
            {
                _outgoing.Add(message);
            }
            else
            {
                _outgoing.AddRange(enumerable);
            }
        }

        public IEnumerable<object> OutgoingMessages()
        {
            return _outgoing;
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
}