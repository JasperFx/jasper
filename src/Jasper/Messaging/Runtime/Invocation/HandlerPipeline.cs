using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        WorkersGraph Workers { get; }
        Task Invoke(Envelope envelope);
        Task InvokeNow(Envelope envelope);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly HandlerGraph _graph;

        private readonly IMissingHandler[] _missingHandlers;
        private readonly IMessagingRoot _root;
        private readonly MessagingSerializationGraph _serializer;

        public HandlerPipeline(MessagingSerializationGraph serializers, HandlerGraph graph, IMessageLogger logger,
            IEnumerable<IMissingHandler> missingHandlers, IMessagingRoot root)
        {
            _serializer = serializers;
            _graph = graph;
            _root = root;
            _missingHandlers = missingHandlers.ToArray();

            Logger = logger;
        }


        public IMessageLogger Logger { get; }

        public WorkersGraph Workers => _graph.Workers;

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


        public async Task InvokeNow(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
                throw new ArgumentOutOfRangeException(nameof(envelope),
                    $"No known handler for message type {envelope.Message.GetType().FullName}");


            var context = _root.ContextFor(envelope);
            envelope.StartTiming();

            try
            {
                await handler.Handle(context);

                // TODO -- what do we do here if this fails? Compensating actions?
                await context.SendAllQueuedOutgoingMessages();

                envelope.MarkCompletion(true);
            }
            catch (Exception e)
            {
                envelope.MarkCompletion(false);
                Logger.LogException(e, message: $"Invocation of {envelope} failed!");
                throw;
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

            envelope.StartTiming();

            try
            {
                deserialize(envelope);
            }
            catch (Exception e)
            {
                envelope.MarkCompletion(false);
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


        private void deserialize(Envelope envelope)
        {
            if (envelope.Message == null) envelope.Message = _serializer.Deserialize(envelope);
        }

        public async Task ProcessMessage(Envelope envelope, IMessageContext context)
        {
            Logger.ExecutionStarted(envelope);

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                await processNoHandlerLogic(envelope, context);
                envelope.MarkCompletion(false);

                // These two lines are important to make the message tracking work
                // if there is no handler
                Logger.ExecutionFinished(envelope);
                Logger.MessageSucceeded(envelope);
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

                context.Envelope.MarkCompletion(true);

                return MessageSucceededContinuation.Instance;
            }
            catch (Exception e)
            {
                context.Envelope.MarkCompletion(false);
                Logger.LogException(e, context.Envelope.Id, "Failure during message processing execution");
                Logger.ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete

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
                try
                {
                    await handler.Handle(envelope, context);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                }

            if (envelope.AckRequested) await context.Advanced.SendAcknowledgement();

            await envelope.Callback.MarkComplete();
        }
    }
}
