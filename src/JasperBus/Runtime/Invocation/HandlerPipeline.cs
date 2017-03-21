using System;
using System.Threading.Tasks;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Model;
using JasperBus.Runtime.Serializers;

namespace JasperBus.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        Task Invoke(Envelope envelope, ChannelNode receiver);
        IBusLogger Logger { get; }
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly IEnvelopeSender _sender;
        private readonly IEnvelopeSerializer _serializer;
        private readonly HandlerGraph _graph;
        private readonly IReplyWatcher _replies;

        public HandlerPipeline(IEnvelopeSender sender, IEnvelopeSerializer serializer, HandlerGraph graph, IReplyWatcher replies, IBusLogger[] loggers)
        {
            _sender = sender;
            _serializer = serializer;
            _graph = graph;
            _replies = replies;

            Logger = BusLogger.Combine(loggers);
        }

        public IBusLogger Logger { get; }

        public async Task Invoke(Envelope envelope, ChannelNode receiver)
        {
            var now = DateTime.UtcNow;

            try
            {
                await invoke(envelope, receiver, now).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogException(e, envelope.CorrelationId);
            }
        }

        private async Task invoke(Envelope envelope, ChannelNode receiver, DateTime now)
        {
            using (var context = new EnvelopeContext(this, envelope, _sender))
            {
                if (envelope.IsDelayed(now))
                {
                    moveToDelayedMessageQueue(envelope, context);
                }
                else if (envelope.ResponseId.IsNotEmpty())
                {
                    completeRequestWithRequestedResponse(envelope, receiver);
                }
                else
                {
                    Logger.Received(envelope);

                    deserialize(envelope, receiver);


                    await ProcessMessage(envelope, context).ConfigureAwait(false);
                }
            }
        }

        private void deserialize(Envelope envelope, ChannelNode receiver)
        {
// TODO -- Not super duper wild about this one.
            if (envelope.Message == null)
            {
                envelope.Message = _serializer.Deserialize(envelope, receiver);
            }
        }

        public async Task ProcessMessage(Envelope envelope, EnvelopeContext context)
        {
            Logger.ExecutionStarted(envelope);

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

                await continuation.Execute(envelope, context, DateTime.UtcNow).ConfigureAwait(false);
            }
        }

        private async Task<IContinuation> executeChain(MessageHandler handler, EnvelopeContext context)
        {
            try
            {
                context.Envelope.Attempts++;

                await handler.Handle(context).ConfigureAwait(false);

                Logger.ExecutionFinished(context.Envelope);

                return ChainSuccessContinuation.Instance;
            }
            catch (Exception e)
            {
                Logger.LogException(e, context.Envelope.CorrelationId, "Failure during message processing execution");
                return context.DetermineContinuation(e, handler.Chain, _graph);
            }
        }

        private void processNoHandlerLogic(Envelope envelope)
        {
            Logger.NoHandlerFor(envelope);
        }

        private void completeRequestWithRequestedResponse(Envelope envelope, ChannelNode receiver)
        {
            try
            {
                deserialize(envelope, receiver);
                _replies.Handle(envelope);
            }
            catch (Exception e)
            {
                Logger.LogException(e, envelope.CorrelationId, "Failure during reply handling.");
            }
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
                context.Logger.LogException(e, envelope.CorrelationId, "Failed to move delayed message to the delayed message queue");
            }
        }
    }
}