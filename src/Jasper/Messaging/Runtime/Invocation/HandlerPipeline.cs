using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        Task Invoke(Envelope envelope);
        Task InvokeNow(Envelope envelope);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly MessagingSerializationGraph _serializer;
        private readonly HandlerGraph _graph;
        private readonly IReplyWatcher _replies;
        private readonly IMessagingRoot _root;

        // TODO -- try to eliminate this dependency
        private readonly IMissingHandler[] _missingHandlers;

        public HandlerPipeline(MessagingSerializationGraph serializers, HandlerGraph graph, IReplyWatcher replies, IMessageLogger logger, IEnumerable<IMissingHandler> missingHandlers, IMessagingRoot root)
        {
            _serializer = serializers;
            _graph = graph;
            _replies = replies;
            _root = root;
            _missingHandlers = missingHandlers.ToArray();

            Logger = logger;
        }


        public IMessageLogger Logger { get; }

        public async Task Invoke(Envelope envelope)
        {
            if (envelope.IsExpired())
            {
                await discardEnvelope(envelope);
                return;
            }


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

        private async Task discardEnvelope(Envelope envelope)
        {
            try
            {
                Logger.DiscardedEnvelope(envelope);
                await envelope.Callback.MarkComplete();
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        private async Task invoke(Envelope envelope, DateTime now)
        {
            var context = _root.ContextFor(envelope);

            if (envelope.ResponseId.IsNotEmpty())
            {
                await completeRequestWithRequestedResponse(envelope);
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


        public async Task InvokeNow(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                throw new ArgumentOutOfRangeException(nameof(envelope), $"No known handler for message type {envelope.Message.GetType().FullName}");
            }

            var context = _root.ContextFor(envelope);
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


        private void deserialize(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                envelope.Message = _serializer.Deserialize(envelope);
            }
        }

        public async Task ProcessMessage(Envelope envelope, IMessageContext context)
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

                await continuation.Execute(context, DateTime.UtcNow).ConfigureAwait(false);
            }
        }

        private async Task<IContinuation> executeChain(MessageHandler handler, IMessageContext context)
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
                if (context.Envelope.Attempts >= handler.Chain.MaximumAttempts) return new MoveToErrorQueue(e);

                return handler.Chain.DetermineContinuation(context.Envelope, e)
                       ?? _graph.DetermineContinuation(context.Envelope, e)
                       ?? new MoveToErrorQueue(e);
            }
        }

        private async Task processNoHandlerLogic(Envelope envelope, IMessageContext context)
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
                await context.Advanced.SendAcknowledgement();
            }

            await envelope.Callback.MarkComplete();
        }

        private Task completeRequestWithRequestedResponse(Envelope envelope)
        {
            try
            {
                deserialize(envelope);
                _replies.Handle(envelope);

                return envelope.Callback.MarkComplete();
            }
            catch (Exception e)
            {
                Logger.LogException(e, envelope.Id, "Failure during reply handling.");
                return envelope.Callback.MoveToErrors(envelope, e);
            }
        }

    }
}
