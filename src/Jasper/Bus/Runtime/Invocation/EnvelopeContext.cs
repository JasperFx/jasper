using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;

namespace Jasper.Bus.Runtime.Invocation
{
    public class EnvelopeContext : IEnvelopeContext
    {
        private readonly HandlerPipeline _pipeline;
        private readonly IEnvelopeSender _sender;
        private readonly List<object> _outgoing = new List<object>();
        private readonly List<object> _inline = new List<object>();

        public EnvelopeContext(HandlerPipeline pipeline, Envelope envelope, IEnvelopeSender sender)
        {
            Envelope = envelope;
            _pipeline = pipeline;
            _sender = sender;
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
            SendOutgoingMessages(Envelope, _outgoing);
        }

        public void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages)
        {
            if (original.AckRequested)
            {
                sendAcknowledgement(original);
            }

            foreach (var o in cascadingMessages)
            {
                SendOutgoingMessage(original, o);
            }
        }

        private void sendAcknowledgement(Envelope original)
        {
            var envelope = new Envelope
            {
                ParentId = original.CorrelationId,
                Destination = original.ReplyUri,
                ResponseId = original.CorrelationId,
                Message = new Acknowledgement {CorrelationId = original.CorrelationId}
            };

            Send(envelope);
        }

        public void SendOutgoingMessage(Envelope original, object o)
        {
            var cascadingEnvelope = o is ISendMyself
                ? o.As<ISendMyself>().CreateEnvelope(original)
                : original.ForResponse(o);

            if (original.AcceptedContentTypes.Any())
            {
                cascadingEnvelope.AcceptedContentTypes = original.AcceptedContentTypes;
            }

            cascadingEnvelope.Callback = original.Callback;

            Send(cascadingEnvelope);
        }

        public void SendFailureAcknowledgement(Envelope original, string message)
        {
            if (original.AckRequested || original.ReplyRequested.IsNotEmpty())
            {
                var envelope = new Envelope
                {
                    ParentId = original.CorrelationId,
                    Destination = original.ReplyUri,
                    ResponseId = original.CorrelationId,
                    Message = new FailureAcknowledgement()
                    {
                        CorrelationId = original.CorrelationId,
                        Message = message
                    },
                    Callback = original.Callback
                };

                Send(envelope);
            }
        }

        public Task Retry(Envelope envelope)
        {
            _outgoing.Clear();
            _inline.Clear();

            return _pipeline.ProcessMessage(envelope, this);
        }

        public IContinuation DetermineContinuation(Exception exception, HandlerChain handlerChain, HandlerGraph graph)
        {
            if (Envelope.Attempts >= handlerChain.MaximumAttempts) return new MoveToErrorQueue(exception);

            return handlerChain.DetermineContinuation(Envelope, exception)
                   ?? graph.DetermineContinuation(Envelope, exception)
                   ?? new MoveToErrorQueue(exception);
        }

        public void Send(Envelope envelope)
        {
            try
            {
                if (envelope.Callback != null && envelope.Callback.SupportsSend)
                {
                    _sender.Send(envelope, envelope.Callback);
                }
                else
                {
                    _sender.Send(envelope);
                }
            }
            catch (Exception e)
            {
                // TODO -- we really, really have to do something here
                Logger.LogException(e, envelope.CorrelationId, "Failure while trying to send a cascading message");
            }
        }
    }
}