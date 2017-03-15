using System;
using System.Threading.Tasks;
using Baseline;
using JasperBus.Configuration;
using JasperBus.ErrorHandling;
using JasperBus.Model;
using JasperBus.Runtime.Serializers;

namespace JasperBus.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        Task Invoke(Envelope envelope, ChannelNode receiver);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly IEnvelopeSender _sender;
        private readonly IEnvelopeSerializer _serializer;
        private readonly HandlerGraph _graph;

        public HandlerPipeline(IEnvelopeSender sender, IEnvelopeSerializer serializer, HandlerGraph graph)
        {
            _sender = sender;
            _serializer = serializer;
            _graph = graph;
        }

        public async Task Invoke(Envelope envelope, ChannelNode receiver)
        {
            var now = DateTime.UtcNow;

            using (var context = new EnvelopeContext(this, envelope))
            {
                if (envelope.IsDelayed(now))
                {
                    moveToDelayedMessageQueue(envelope, context);
                }
                else if (envelope.ResponseId.IsNotEmpty())
                {
                    // TODO -- actually do something here;)
                    completeRequestWithRequestedResponse(envelope);
                }
                else
                {
                    // Not super duper wild about this one.
                    if (envelope.Message == null)
                    {
                        envelope.Message = _serializer.Deserialize(envelope, receiver);
                    }


                    await ProcessMessage(envelope, context);
                }
            }
        }

        public async Task ProcessMessage(Envelope envelope, EnvelopeContext context)
        {
            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                processNoHandlerLogic(envelope);
            }
            else
            {
                // TODO -- have the EnvelopeContext.Retry be able to skip right down
                // to the executeChain method here
                var continuation = await executeChain(handler, context).ConfigureAwait(false);

                // TODO -- should continuations be async too? -- YES.
                await continuation.Execute(envelope, context, DateTime.UtcNow).ConfigureAwait(false);
            }
        }

        private async Task<IContinuation> executeChain(MessageHandler handler, EnvelopeContext context)
        {
            try
            {
                context.Envelope.Attempts++;

                await handler.Handle(context).ConfigureAwait(false);

                return ChainSuccessContinuation.Instance;
            }
            catch (Exception e)
            {
                return context.DetermineContinuation(e, handler.Chain, _graph);
            }
        }

        private void processNoHandlerLogic(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        private void completeRequestWithRequestedResponse(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        // TODO -- think this is gonna die
        private static void moveToDelayedMessageQueue(Envelope envelope, EnvelopeContext context)
        {
            try
            {
                envelope.Callback.MoveToDelayedUntil(envelope.ExecutionTime.Value);
            }
            catch (Exception e)
            {
                envelope.Callback.MarkFailed(e);
                context.Error(envelope.CorrelationId, "Failed to move delayed message to the delayed message queue", e);
            }
        }
    }
}