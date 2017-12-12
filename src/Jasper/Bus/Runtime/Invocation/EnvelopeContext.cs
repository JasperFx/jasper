using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Delayed;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;

namespace Jasper.Bus.Runtime.Invocation
{
    public class EnvelopeContext : IEnvelopeContext
    {
        private readonly HandlerPipeline _pipeline;
        private readonly IServiceBus _sender;
        private readonly List<object> _outgoing = new List<object>();
        private readonly List<object> _inline = new List<object>();

        public EnvelopeContext(HandlerPipeline pipeline, Envelope envelope, IServiceBus sender)
        {
            Envelope = envelope;
            _pipeline = pipeline;
            _sender = sender ?? throw new ArgumentNullException();
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

        public Task SendAllQueuedOutgoingMessages()
        {
            return SendOutgoingMessages(Envelope, _outgoing);
        }

        public async Task SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages)
        {
            if (original.AckRequested)
            {
                await SendAcknowledgement(original);
            }

            foreach (var o in cascadingMessages)
            {
                await SendOutgoingMessage(original, o);
            }
        }

        public Task SendAcknowledgement(Envelope original)
        {
            var envelope = new Envelope
            {
                ParentId = original.Id,
                Destination = original.ReplyUri,
                ResponseId = original.Id,
                Message = new Acknowledgement {CorrelationId = original.Id}
            };

            return Send(envelope);
        }

        public Task SendOutgoingMessage(Envelope original, object o)
        {
            var cascadingEnvelope = o is ISendMyself
                ? o.As<ISendMyself>().CreateEnvelope(original)
                : original.ForResponse(o);


            cascadingEnvelope.AcceptedContentTypes = original.AcceptedContentTypes;


            cascadingEnvelope.Callback = original.Callback;

            return Send(cascadingEnvelope);
        }

        public Task SendFailureAcknowledgement(Envelope original, string message)
        {
            if (original.AckRequested || original.ReplyRequested.IsNotEmpty())
            {
                var envelope = new Envelope
                {
                    ParentId = original.Id,
                    Destination = original.ReplyUri,
                    ResponseId = original.Id,
                    Message = new FailureAcknowledgement()
                    {
                        CorrelationId = original.Id,
                        Message = message
                    },
                    Callback = original.Callback
                };

                return Send(envelope);
            }

            return Task.CompletedTask;
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

        public Task Send(Envelope envelope)
        {
            try
            {
                return _sender.Publish(envelope);
            }
            catch (Exception e)
            {
                Logger.LogException(e, envelope.Id, "Failure while trying to send a cascading message");
            }

            return Task.CompletedTask;
        }
    }
}
