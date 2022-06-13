using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Logging;

namespace Jasper.Runtime.Handlers;

internal enum InvokeResult
{
    Success,
    TryAgain
}

internal class Executor
{
    private readonly MessageHandler _handler;
    private readonly TimeSpan _timeout;
    private readonly IReadOnlyList<ExceptionRule> _rules;
    private readonly IMessageLogger _logger;

    public static Executor? Build(IJasperRuntime runtime, HandlerGraph handlerGraph, Type messageType)
    {
        var handler = handlerGraph.HandlerFor(messageType);
        if (handler == null) return null; // TODO: later let's have it return an executor that calls missing handlers

        var timeoutSpan = handler.Chain!.DetermineMessageTimeout(runtime.Options);
        var rules = handler.Chain.Retries.CombineRules(handlerGraph.Retries);
        return new Executor(runtime, handler, rules, timeoutSpan);
    }

    public Executor(IJasperRuntime runtime, MessageHandler handler, IEnumerable<ExceptionRule> rules, TimeSpan timeout)
    {
        _handler = handler;
        _timeout = timeout;
        _rules = rules.ToArray();
        _logger = runtime.MessageLogger;
    }

    public async Task<IContinuation> ExecuteAsync(IExecutionContext context, CancellationToken cancellation)
    {
        context.Envelope!.Attempts++;

        using var timeout = new CancellationTokenSource(_timeout);
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellation);

        try
        {
            await _handler.HandleAsync(context, combined.Token);
            return MessageSucceededContinuation.Instance;
        }
        catch (Exception e)
        {
            _logger.LogException(e, context.Envelope!.Id, "Failure during message processing execution");
            _logger
                .ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete

            var match = _rules.FirstOrDefault(x => x.Filter(e));

            return match?.Action(context.Envelope, e) ?? new MoveToErrorQueue(e);
        }
    }

    public async Task<InvokeResult> InvokeAsync(IExecutionContext context, CancellationToken cancellation)
    {
        try
        {
            await _handler.HandleAsync(context, cancellation);
            return InvokeResult.Success;
        }
        catch (Exception e)
        {
            _logger.LogException(e, message: $"Invocation of {context.Envelope} failed!");
            _logger
                .ExecutionFinished(context.Envelope); // Need to do this to make the MessageHistory complete

            var match = _rules.FirstOrDefault(x => x.Filter(e));
            if (match == null) throw;

            if (match.Action(context.Envelope, e) is RetryNowContinuation retry)
            {
                if (retry.Delay.HasValue)
                {
                    await Task.Delay(retry.Delay.Value, cancellation).ConfigureAwait(false);
                }

                return InvokeResult.TryAgain;
            }

            throw;
        }
    }
}
