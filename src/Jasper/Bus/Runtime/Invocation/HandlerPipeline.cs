using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Transports.Lightweight;
using Jasper.Bus.Transports.Loopback;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        Task Invoke(Envelope envelope);
        IBusLogger Logger { get; }
        Task InvokeNow(object message);
        Task InvokeNow(Envelope envelope);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly IEnvelopeSender _sender;
        private readonly SerializationGraph _serializer;
        private readonly HandlerGraph _graph;
        private readonly IReplyWatcher _replies;
        private readonly IDelayedJobProcessor _delayedJobs;
        private readonly IChannelGraph _channels;
        private readonly IMissingHandler[] _missingHandlers;

        public HandlerPipeline(IEnvelopeSender sender, SerializationGraph serializers, HandlerGraph graph, IReplyWatcher replies, IDelayedJobProcessor delayedJobs, CompositeLogger logger, IChannelGraph channels, IEnumerable<IMissingHandler> missingHandlers)
        {
            _sender = sender;
            _serializer = serializers;
            _graph = graph;
            _replies = replies;
            _delayedJobs = delayedJobs;
            _channels = channels;
            _missingHandlers = missingHandlers.ToArray();

            Logger = logger;
        }

        public IBusLogger Logger { get; }

        public async Task Invoke(Envelope envelope)
        {
            var now = DateTime.UtcNow;

            try
            {
                await invoke(envelope, now).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Gotta get the message out of here because it's something that
                // could never be handled
                await envelope.Callback.MoveToErrors(new ErrorReport(envelope, e));
                Logger.LogException(e, envelope.CorrelationId);
            }
        }

        private async Task invoke(Envelope envelope, DateTime now)
        {
            using (var context = new EnvelopeContext(this, envelope, _sender, _delayedJobs))
            {
                if (envelope.IsDelayed(now))
                {
                    await moveToDelayedMessageQueue(envelope, context);
                }
                else if (envelope.ResponseId.IsNotEmpty())
                {
                    completeRequestWithRequestedResponse(envelope);
                }
                else
                {
                    try
                    {
                        deserialize(envelope);
                    }
                    catch (Exception e)
                    {
                        Logger.MessageFailed(envelope, e);
                        await envelope.Callback.MoveToErrors(new ErrorReport(envelope, e));
                        return;
                    }
                    finally
                    {
                        Logger.Received(envelope);
                    }

                    await ProcessMessage(envelope, context).ConfigureAwait(false);
                }
            }
        }

        public Task InvokeNow(object message)
        {
            var envelope = new Envelope
            {
                Message = message,
                Callback = new LightweightCallback(_channels.DefaultRetryChannel)
            };

            return InvokeNow(envelope);
        }

        public async Task InvokeNow(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), $"No known handler for message type {envelope.Message.GetType().FullName}");
            }

            using (var context = new EnvelopeContext(this, envelope, _sender, _delayedJobs))
            {
                try
                {
                    await handler.Handle(context);

                    // TODO -- what do we do here if this fails? Compensating actions?
                    await context.SendAllQueuedOutgoingMessages();
                }
                catch (Exception e)
                {
                    Logger.LogException(e, $"Invocation of {envelope} failed!");
                    throw;
                }
            }
        }


        private void deserialize(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                envelope.Message = _serializer.Deserialize(envelope);
            }
        }

        public async Task ProcessMessage(Envelope envelope, EnvelopeContext context)
        {
            Logger.ExecutionStarted(envelope);

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                await processNoHandlerLogic(envelope, context);
            }
            else
            {
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

                return MessageSucceededContinuation.Instance;
            }
            catch (Exception e)
            {
                Logger.LogException(e, context.Envelope.CorrelationId, "Failure during message processing execution");
                return context.DetermineContinuation(e, handler.Chain, _graph);
            }
        }

        private async Task processNoHandlerLogic(Envelope envelope, EnvelopeContext context)
        {
            Logger.NoHandlerFor(envelope);

            foreach (var handler in _missingHandlers)
            {
                try
                {
                    await handler.Handle(envelope, context);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }
            }

            if (envelope.AckRequested)
            {
                await context.SendAcknowledgement(envelope);
            }
        }

        private void completeRequestWithRequestedResponse(Envelope envelope)
        {
            try
            {
                deserialize(envelope);
                _replies.Handle(envelope);
            }
            catch (Exception e)
            {
                Logger.LogException(e, envelope.CorrelationId, "Failure during reply handling.");
            }
        }

        private async Task moveToDelayedMessageQueue(Envelope envelope, EnvelopeContext context)
        {
            try
            {
                envelope.Attempts++;
                _delayedJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
                await envelope.Callback.MarkSuccessful();
            }
            catch (Exception e)
            {
                if (envelope.Attempts >= 3)
                {
                    await envelope.Callback.MarkFailed(e);
                    context.Logger.LogException(e, envelope.CorrelationId, "Failed to move delayed message to the delayed message queue");
                }

                var continuation = _graph.DetermineContinuation(envelope, e) ?? new MoveToErrorQueue(e);
                await continuation.Execute(envelope, context, DateTime.UtcNow);
            }
        }
    }
}
