using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime.Handlers;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Util;
using Polly;

namespace Jasper.Runtime
{
    public interface IHandlerPipeline
    {

        Task Invoke(Envelope envelope);
        Task InvokeNow(Envelope envelope);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly HandlerGraph _graph;

        private readonly IMissingHandler[] _missingHandlers;
        private readonly IMessagingRoot _root;
        private readonly MessagingSerializationGraph _serializer;

        private ImHashMap<Type, Func<IMessageContext, Task<IContinuation>>> _executors =
            ImHashMap<Type, Func<IMessageContext, Task<IContinuation>>>.Empty;

        private readonly CancellationToken _cancellation;


        public HandlerPipeline(MessagingSerializationGraph serializers, HandlerGraph graph, IMessageLogger logger,
            IEnumerable<IMissingHandler> missingHandlers, IMessagingRoot root)
        {
            _serializer = serializers;
            _graph = graph;
            _root = root;
            _missingHandlers = missingHandlers.ToArray();
            _cancellation = root.Cancellation;

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
                await envelope.MoveToErrors(_root, e);
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
                await handler.Handle(context, _cancellation);

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
                await envelope.Callback.Complete();
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
                await envelope.MoveToErrors(_root, e);
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

            Func<IMessageContext, Task<IContinuation>> executor = null;
            try
            {
                executor = ExecutorFor(envelope.Message.GetType());
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw;
            }

            if (executor == null)
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
                var continuation = await executor(context);
                Logger.ExecutionFinished(envelope);

                await continuation.Execute(_root, context, DateTime.UtcNow);
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

            await envelope.Callback.Complete();
        }

        public Func<IMessageContext, Task<IContinuation>> ExecutorFor(Type messageType)
        {
            if (_executors.TryFind(messageType, out var executor)) return executor;

            var handler = _graph.HandlerFor(messageType);

            // Memoize the null
            if (handler == null)
            {
                _executors = _executors.AddOrUpdate(messageType, null);
                return null;
            }

            var policy = handler.Chain.Retries.BuildPolicy(_graph.Retries);

            if (policy == null)
            {
                executor = async messageContext =>
                {
                    messageContext.Envelope.Attempts++;

                    try
                    {
                        try
                        {
                            await handler.Handle(messageContext, _cancellation);
                        }
                        catch (Exception e)
                        {
                            MarkFailure(messageContext, e);
                            return new MoveToErrorQueue(e);
                        }

                        messageContext.Envelope.MarkCompletion(true);

                        return MessageSucceededContinuation.Instance;
                    }
                    catch (Exception e)
                    {
                        MarkFailure(messageContext, e);
                        return new MoveToErrorQueue(e);
                    }
                };
            }
            else
            {
                executor = async messageContext =>
                {
                    messageContext.Envelope.Attempts++;
                    messageContext.Envelope.StartTiming();

                    try
                    {
                        var pollyContext = new Context();
                        pollyContext.Store(messageContext);



                        return await policy.ExecuteAsync(async c =>
                        {
                            var context = c.MessageContext();
                            try
                            {
                                await handler.Handle(context, _cancellation);
                            }
                            catch (Exception e)
                            {
                                MarkFailure(messageContext, e);
                                throw;
                            }

                            messageContext.Envelope.MarkCompletion(true);

                            return MessageSucceededContinuation.Instance;
                        }, pollyContext);
                    }
                    catch (Exception e)
                    {
                        MarkFailure(messageContext, e);
                        return new MoveToErrorQueue(e);
                    }
                };
            }





            _executors = _executors.AddOrUpdate(messageType, executor);

            return executor;
        }

        internal static void MarkFailure(IMessageContext context, Exception ex)
        {
            context.Envelope.MarkCompletion(false);
            context.Advanced.Logger.LogException(ex, context.Envelope.Id, "Failure during message processing execution");
            context.Advanced.Logger.ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete
        }
    }
}
