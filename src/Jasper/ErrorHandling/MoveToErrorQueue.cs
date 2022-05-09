﻿using System;
using System.Threading.Tasks;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public class MoveToErrorQueue : IContinuation
{
    private readonly Exception _exception;

    public MoveToErrorQueue(Exception exception)
    {
        _exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public async ValueTask ExecuteAsync(IExecutionContext execution,
        DateTimeOffset now)
    {
        await execution.SendFailureAcknowledgementAsync(execution.Envelope!,
            $"Moved message {execution.Envelope!.Id} to the Error Queue.\n{_exception}");

        await execution.MoveToDeadLetterQueueAsync(_exception);

        execution.Logger.MessageFailed(execution.Envelope, _exception);
        execution.Logger.MovedToErrorQueue(execution.Envelope, _exception);
    }

    public override string ToString()
    {
        return "Move to Error Queue";
    }

    protected bool Equals(MoveToErrorQueue other)
    {
        return Equals(_exception, other._exception);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((MoveToErrorQueue)obj);
    }

    public override int GetHashCode()
    {
        return _exception.GetHashCode();
    }
}
