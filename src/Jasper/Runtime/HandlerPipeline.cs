using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.ImTools;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime.Handlers;
using Jasper.Serialization;
using Jasper.Transports;
using Microsoft.Extensions.ObjectPool;
using Polly;

namespace Jasper.Runtime
{
    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly string _activityName;

        private readonly CancellationToken _cancellation;
        private readonly ObjectPool<ExecutionContext> _contextPool;
        private readonly HandlerGraph _graph;
        private readonly NoHandlerContinuation _noHandlers;

        private readonly IMessagingRoot _root;
        private readonly MessagingSerializationGraph _serializer;

        private ImHashMap<Type, Func<IExecutionContext, Task<IContinuation>>> _executors =
            ImHashMap<Type, Func<IExecutionContext, Task<IContinuation>>>.Empty;


        public HandlerPipeline(MessagingSerializationGraph serializers, HandlerGraph graph, IMessageLogger logger,
            NoHandlerContinuation noHandlers, IMessagingRoot root, ObjectPool<ExecutionContext> contextPool)
        {
            _serializer = serializers;
            _graph = graph;
            _noHandlers = noHandlers;
            _root = root;
            _contextPool = contextPool;
            _cancellation = root.Cancellation;

            Logger = logger;
            _activityName = $"{_root.Settings.ServiceName} process";
        }


        public IMessageLogger Logger { get; }

        public async Task Invoke(Envelope envelope, IChannelCallback channel)
        {
            var activity = JasperTracing.StartExecution(envelope);
            try
            {
                var context = _contextPool.Get();
                context.ReadEnvelope(envelope, channel);

                try
                {
                    var continuation = await execute(context, envelope);
                    await continuation.Execute(context, DateTime.UtcNow);
                }
                catch (Exception e)
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


        public async Task InvokeNow(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentNullException(nameof(envelope.Message));
            }

            // TODO -- probably pull a lot of this into a separate method
            using var activity = JasperTracing.ActivitySource.StartActivity(_activityName);
            activity.SetTag(JasperTracing.MessagingSystem, JasperTracing.Local);
            activity.SetTag(JasperTracing.MessagingMessageId, envelope.Id);
            activity.SetTag(JasperTracing.MessagingConversationId, envelope.CorrelationId);
            activity.SetTag(JasperTracing.MessageType, envelope.MessageType); // Jasper specific
            activity.SetParentId(envelope.CausationId.ToString());

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
                await handler.Handle(context, _cancellation);

                await context.SendAllQueuedOutgoingMessages();
            }
            catch (Exception e)
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

            // Try to deserialize
            try
            {
                envelope.Message ??= _serializer.Deserialize(envelope);
            }
            catch (Exception e)
            {
                return new MoveToErrorQueue(e);
            }
            finally
            {
                Logger.Received(envelope);
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
                        catch (Exception e)
                        {
                            MarkFailure(messageContext, e);
                            return new MoveToErrorQueue(e);
                        }

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

        internal void MarkFailure(IExecutionContext context, Exception ex)
        {
            _root.MessageLogger.LogException(ex, context.Envelope.Id, "Failure during message processing execution");
            _root.MessageLogger
                .ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete
        }
    }
}
