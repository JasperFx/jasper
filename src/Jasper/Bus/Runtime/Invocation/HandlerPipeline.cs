using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Delayed;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        Task Invoke(Envelope envelope);
        Task InvokeNow(Envelope envelope);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly BusMessageSerializationGraph _serializer;
        private readonly HandlerGraph _graph;
        private readonly IReplyWatcher _replies;
        private readonly Lazy<IServiceBus> _bus;
        private readonly IMissingHandler[] _missingHandlers;

        public HandlerPipeline(BusMessageSerializationGraph serializers, HandlerGraph graph, IReplyWatcher replies, CompositeLogger logger, IEnumerable<IMissingHandler> missingHandlers, Lazy<IServiceBus> bus)
        {
            _serializer = serializers;
            _graph = graph;
            _replies = replies;
            _bus = bus;
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
                await envelope.Callback.MoveToErrors(envelope, e);
                Logger.LogException(e, envelope.Id);
            }
        }

        private async Task invoke(Envelope envelope, DateTime now)
        {
            using (var context = new EnvelopeContext(this, envelope, _bus.Value))
            {
                if (envelope.ResponseId.IsNotEmpty())
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
                        await envelope.Callback.MoveToErrors(envelope, e);
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


        public async Task InvokeNow(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), $"No known handler for message type {envelope.Message.GetType().FullName}");
            }

            using (var context = new EnvelopeContext(this, envelope, _bus.Value))
            {
                try
                {
                    await handler.Handle(context);

                    // TODO -- what do we do here if this fails? Compensating actions?
                    await context.SendAllQueuedOutgoingMessages();
                }
                catch (Exception e)
                {
                    Logger.LogException(e, message:$"Invocation of {envelope} failed!");
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
                Logger.LogException(e, context.Envelope.Id, "Failure during message processing execution");
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
                Logger.LogException(e, envelope.Id, "Failure during reply handling.");
            }
        }

    }
}
