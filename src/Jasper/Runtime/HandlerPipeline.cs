using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Baseline.ImTools;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime.Handlers;
using Jasper.Transports;
using Microsoft.Extensions.ObjectPool;
using Polly;

namespace Jasper.Runtime;

public class HandlerPipeline : IHandlerPipeline
{
    private readonly CancellationToken _cancellation;
    private readonly ObjectPool<ExecutionContext> _contextPool;
    private readonly HandlerGraph _graph;
    private readonly NoHandlerContinuation _noHandlers;

    private readonly IJasperRuntime _runtime;


    private readonly AdvancedSettings _settings;

    private ImHashMap<Type, Func<IExecutionContext, Task<IContinuation>>?> _executors =
        ImHashMap<Type, Func<IExecutionContext, Task<IContinuation>>?>.Empty;


    public HandlerPipeline(HandlerGraph graph, IMessageLogger logger,
        NoHandlerContinuation noHandlers, IJasperRuntime runtime, ObjectPool<ExecutionContext> contextPool)
    {
        _graph = graph;
        _noHandlers = noHandlers;
        _runtime = runtime;
        _contextPool = contextPool;
        _cancellation = runtime.Cancellation;

        Logger = logger;

        _settings = runtime.Advanced;
    }

    public IMessageLogger Logger { get; }

    public Task InvokeAsync(Envelope envelope, IChannelCallback channel)
    {
        using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryProcessSpanName!, envelope);

        return InvokeAsync(envelope, channel, activity);
    }

    public async Task InvokeAsync(Envelope envelope, IChannelCallback channel, Activity activity)
    {
        try
        {
            var context = _contextPool.Get();
            context.ReadEnvelope(envelope, channel);

            try
            {
                // TODO -- pass the activity into IContinuation?
                var continuation = await executeAsync(context, envelope);
                await continuation.ExecuteAsync(context, _runtime, DateTimeOffset.Now);
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


    public async Task InvokeNowAsync(Envelope envelope, CancellationToken cancellation = default)
    {
        if (envelope.Message == null)
        {
            throw new ArgumentNullException(nameof(envelope.Message));
        }

        using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryProcessSpanName!, envelope);

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
            await handler.HandleAsync(context, cancellation);

            await context.FlushOutgoingMessagesAsync();
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

    private async Task<IContinuation> executeAsync(IExecutionContext context, Envelope envelope)
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
                var serializer = envelope.Serializer ?? _runtime.Options.DetermineSerializer(envelope);

                if (envelope.Data == null)
                    throw new ArgumentOutOfRangeException(nameof(envelope),
                        "Envelope does not have a message or deserialized message data");

                if (envelope.MessageType == null)
                    throw new ArgumentOutOfRangeException(nameof(envelope),
                        "The envelope has no Message or MessageType name");

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
            throw new ArgumentNullException("envelope", $"{nameof(envelope.Message)} is missing");
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

    public Func<IExecutionContext, Task<IContinuation>>? ExecutorFor(Type messageType)
    {
        if (_executors.TryFind(messageType, out var executor))
        {
            return executor;
        }

        var handler = _graph.HandlerFor(messageType);

        // Memoize the null
        if (handler == null)
        {
            _executors = _executors.AddOrUpdate(messageType, null)!;
            return null;
        }

        var policy = handler.Chain!.Retries.BuildPolicy(_graph.Retries);

        var timeoutSpan = handler.Chain.DetermineMessageTimeout(_runtime.Options);

        if (policy == null)
        {
            executor = async messageContext =>
            {
                messageContext.Envelope!.Attempts++;

                using var timeout = new CancellationTokenSource(timeoutSpan);
                using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, _cancellation);

                try
                {
                    try
                    {
                        await handler.HandleAsync(messageContext, combined.Token);
                    }
                    catch (Exception e)
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
                messageContext.Envelope!.Attempts++;

                try
                {
                    var pollyContext = new Context();
                    pollyContext.Store(messageContext);


                    return await policy.ExecuteAsync(async c =>
                    {
                        var context = c.MessageContext();
                        try
                        {
                            await handler.HandleAsync(context, _cancellation);
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

    internal void MarkFailure(IExecutionContext context, Exception ex)
    {
        _runtime.MessageLogger.LogException(ex, context.Envelope!.Id, "Failure during message processing execution");
        _runtime.MessageLogger
            .ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete
    }
}
