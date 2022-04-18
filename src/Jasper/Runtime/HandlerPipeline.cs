using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Baseline.ImTools;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime.Handlers;
using Jasper.Transports;
using Jasper.Util;
using Microsoft.Extensions.ObjectPool;
using Polly;

namespace Jasper.Runtime
{
    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly CancellationToken _cancellation;
        private readonly ObjectPool<ExecutionContext> _contextPool;
        private readonly HandlerGraph _graph;
        private readonly NoHandlerContinuation _noHandlers;

        private readonly IJasperRuntime _root;

        private ImHashMap<Type, Func<IExecutionContext, Task<IContinuation>>> _executors =
            ImHashMap<Type, Func<IExecutionContext, Task<IContinuation>>>.Empty;


        private readonly AdvancedSettings _settings;


        public HandlerPipeline(HandlerGraph graph, IMessageLogger logger,
            NoHandlerContinuation noHandlers, IJasperRuntime root, ObjectPool<ExecutionContext> contextPool)
        {
            _graph = graph;
            _noHandlers = noHandlers;
            _root = root;
            _contextPool = contextPool;
            _cancellation = root.Cancellation;

            Logger = logger;

            _settings = root.Settings;
        }

        public IMessageLogger Logger { get; }

        public Task Invoke(Envelope envelope, IChannelCallback channel)
        {
            using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryProcessSpanName, envelope, ActivityKind.Internal);

            return Invoke(envelope, channel, activity);
        }

        public async Task Invoke(Envelope envelope, IChannelCallback channel, Activity activity)
        {
            try
            {
                var context = _contextPool.Get();
                context.ReadEnvelope(envelope, channel);

                try
                {
                    // TODO -- pass the activity into IContinuation?
                    var continuation = await execute(context, envelope);
                    await continuation.Execute(context, DateTime.UtcNow);
                }
                catch (Exception? e)
                {
                    // TODO -- gotta do something on the envelope to get it out of the transport

                    // Gotta get the message out of here because it's something that
                    // could never be handled
                    Logger.LogException(e, envelope.Id);
                }
                finally
                {
                    _contextPool.Return(context);
                }
            }
            finally
            {
                activity.Stop();
            }
        }


        public async Task InvokeNow(Envelope envelope, CancellationToken cancellation = default)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentNullException(nameof(envelope.Message));
            }

            using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryProcessSpanName, envelope);

            var handler = _graph.HandlerFor(envelope.Message.GetType());
            if (handler == null)
            {
                // TODO -- mark it as unhandled on the activity
                throw new ArgumentOutOfRangeException(nameof(envelope),
                    $"No known handler for message type {envelope.Message.GetType().FullName}");
            }

            var context = _contextPool.Get();
            context.ReadEnvelope(envelope, InvocationCallback.Instance);

            try
            {
                await handler.Handle(context, cancellation);

                await context.SendAllQueuedOutgoingMessages();
            }
            catch (Exception? e)
            {
                Logger.LogException(e, message: $"Invocation of {envelope} failed!");
                throw;
            }
            finally
            {
                _contextPool.Return(context);
            }
        }

        private async Task<IContinuation> execute(IExecutionContext context, Envelope envelope)
        {
            if (envelope.IsExpired())
            {
                return DiscardExpiredEnvelope.Instance;
            }

            if (envelope.Message == null)
            {
                // Try to deserialize
                try
                {
                    var serializer = envelope.Serializer ?? _root.Options.DetermineSerializer(envelope);
                    envelope.Message = _graph.TryFindMessageType(envelope.MessageType, out var messageType)
                        ? serializer.ReadFromData(messageType, envelope.Data)
                        : serializer.ReadFromData(envelope.Data);

                    if (envelope.Message == null)
                    {
                        return new MoveToErrorQueue(new InvalidOperationException(
                            "No message body could be de-serialized from the raw data in this envelope"));
                    }
                }
                catch (Exception? e)
                {
                    return new MoveToErrorQueue(e);
                }
                finally
                {
                    Logger.Received(envelope);
                }
        }

        if (envelope.Message == null)
            {
                throw new ArgumentNullException("envelope.Message");
            }

            Logger.ExecutionStarted(envelope);

            var executor = ExecutorFor(envelope.Message.GetType());
            if (executor == null)
            {
                return _noHandlers;
            }

            var continuation = await executor(context);
            Logger.ExecutionFinished(envelope);

            return continuation;
        }

        public Func<IExecutionContext, Task<IContinuation>> ExecutorFor(Type messageType)
        {
            if (_executors.TryFind(messageType, out var executor))
            {
                return executor;
            }

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
                        catch (Exception? e)
                        {
                            MarkFailure(messageContext, e);
                            return new MoveToErrorQueue(e);
                        }

                        return MessageSucceededContinuation.Instance;
                    }
                    catch (Exception? e)
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
                            catch (Exception? e)
                            {
                                MarkFailure(messageContext, e);
                                throw;
                            }

                            return MessageSucceededContinuation.Instance;
                        }, pollyContext);
                    }
                    catch (Exception? e)
                    {
                        MarkFailure(messageContext, e);
                        return new MoveToErrorQueue(e);
                    }
                };
            }


            _executors = _executors.AddOrUpdate(messageType, executor);

            return executor;
        }

        internal void MarkFailure(IExecutionContext context, Exception? ex)
        {
            _root.MessageLogger.LogException(ex, context.Envelope.Id, "Failure during message processing execution");
            _root.MessageLogger
                .ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete
        }
    }
}
