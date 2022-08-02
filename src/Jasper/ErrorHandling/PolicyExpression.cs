using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.ErrorHandling.New;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;


public interface IAdditionalActions : IFailureActions
{
    /// <summary>
    /// Define actions to take upon subsequent failures
    /// </summary>
    IFailureActions Then { get; }

    /// <summary>
    /// Pause all processing for the specified time. Will also requeue the
    /// failed message that caused this to trip off
    /// </summary>
    /// <param name="pauseTime"></param>
    IAdditionalActions AndPauseProcessing(TimeSpan pauseTime);


    /// <summary>
    /// Perform a user defined action as well as the initial action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    IAdditionalActions AlsoDo(Func<IJasperRuntime, IExecutionContext, Exception, ValueTask> action,
        string description = "User supplied");
}

internal class FailureActions : IAdditionalActions
{
    private readonly FailureRule _rule;
    private readonly List<FailureSlot> _slots = new List<FailureSlot>();

    public FailureActions(IExceptionMatch match, FailureRuleCollection parent)
    {
        _rule = new FailureRule(match);
        parent.Add(_rule);


    }

    public IAdditionalActions MoveToErrorQueue()
    {
        var slot = _rule.AddSlot(new MoveToErrorQueueSource());
        _slots.Add(slot);
        return this;
    }

    public IAdditionalActions Requeue(int maxAttempts = 3)
    {
        for (int i = 0; i < maxAttempts - 1; i++)
        {
            var slot = _rule.AddSlot(RequeueContinuation.Instance);
            _slots.Add(slot);
        }

        return this;
    }

    public IAdditionalActions Discard()
    {
        var slot = _rule.AddSlot(DiscardEnvelope.Instance);
        _slots.Add(slot);
        return this;
    }

    public IAdditionalActions ScheduleRetry(params TimeSpan[] delays)
    {
        if (!delays.Any())
        {
            throw new InvalidOperationException("You must specify at least one delay time");
        }

        for (int i = 0; i < delays.Length; i++)
        {
            var slot = _rule.AddSlot(new ScheduledRetryContinuation(delays[i]));
            _slots.Add(slot);
        }

        return this;
    }

    public IAdditionalActions RetryNow(int maxAttempts = 3)
    {
        if (maxAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        for (int i = 0; i < maxAttempts - 1; i++)
        {
            var slot = _rule.AddSlot(RetryInlineContinuation.Instance);
            _slots.Add(slot);
        }

        return this;
    }

    public IAdditionalActions RetryWithCooldown(params TimeSpan[] delays)
    {
        if (!delays.Any())
        {
            throw new InvalidOperationException("You must specify at least one delay time");
        }

        for (int i = 0; i < delays.Length; i++)
        {
            var slot = _rule.AddSlot(new RetryInlineContinuation(delays[i]));
            _slots.Add(slot);
        }

        return this;
    }

    public IFailureActions Then
    {
        get
        {
            _slots.Clear();
            return this;
        }
    }

    public IAdditionalActions AndPauseProcessing(TimeSpan pauseTime)
    {
        foreach (var slot in _slots)
        {
            slot.AddAdditionalSource(new PauseListenerContinuation(pauseTime));
        }
        return this;
    }

    public IAdditionalActions AlsoDo(Func<IJasperRuntime, IExecutionContext, Exception, ValueTask> action, string description = "User supplied")
    {
        throw new NotImplementedException();
    }
}



public interface IFailureActions
{
    /// <summary>
    ///     Immediately move the message to the error queue when the exception
    ///     caught matches this criteria
    /// </summary>
    IAdditionalActions MoveToErrorQueue();

    /// <summary>
    ///     Requeue the message back to the incoming transport, with the message being
    ///     dead lettered when the maximum number of attempts is reached
    /// </summary>
    /// <param name="maxAttempts">The maximum number of attempts to process the message. The default is 3</param>
    IAdditionalActions Requeue(int maxAttempts = 3);

    /// <summary>
    ///     Discard the message without any further attempt to process the message
    /// </summary>
    IAdditionalActions Discard();


    /// <summary>
    ///     Schedule the message for additional attempts with a delay. Use this
    ///     method to effect an "exponential backoff" policy
    /// </summary>
    /// <param name="delays"></param>
    /// <exception cref="InvalidOperationException"></exception>
    IAdditionalActions ScheduleRetry(params TimeSpan[] delays);

    /// <summary>
    ///     Retry the message a maximum number of attempts without any delay
    ///     or moving the message back to the original queue
    /// </summary>
    /// <param name="maxAttempts"></param>
    IAdditionalActions RetryNow(int maxAttempts = 3);

    /// <summary>
    /// Retry message failures a define number of times with user-specified cooldown times
    /// between events. This allows for "exponential backoff" strategies
    /// </summary>
    /// <param name="delays"></param>
    /// <param name="maxAttempts"></param>
    IAdditionalActions RetryWithCooldown(params TimeSpan[] delays);
}

public class PolicyExpression : IFailureActions
{
    private readonly FailureRuleCollection _parent;

    private IExceptionMatch _match;

    internal PolicyExpression(FailureRuleCollection parent, IExceptionMatch match)
    {
        _parent = parent;
        _match = match;
    }

    /// <summary>
    /// Specifies that the exception message must contain this fragment. The check is case insensitive.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public PolicyExpression AndMessageContains(string text)
    {
        _match.And(new MessageContains(text));
        return this;
    }


    /// <summary>
    ///     Specifies an additional type of exception that this policy can handle.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyExpression Or<TException>() where TException : Exception
    {
        _match = _match.Or(new TypeMatch<TException>());

        return this;
    }

    /// <summary>
    ///     Specifies an additional type of exception that this policy can handle with additional filters on this exception
    ///     type.
    /// </summary>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <param name="description">Optional description of the filter for diagnostic purposes</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyExpression Or(Func<Exception, bool> exceptionPredicate, string description = "User supplied filter")
    {
        _match = _match.Or(new UserSupplied(exceptionPredicate, description));
        return this;
    }


    /// <summary>
    ///     Specifies an additional type of exception that this policy can handle with additional filters on this exception
    ///     type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <param name="description">Optional description of the filter for diagnostic purposes</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyExpression Or<TException>(Func<TException, bool> exceptionPredicate, string description = "User supplied filter")
        where TException : Exception
    {
        _match = _match.Or(new UserSupplied<TException>(exceptionPredicate, description));
        return this;
    }

    /// <summary>
    ///     Specifies an additional type of exception that this policy can handle if found as an InnerException of a regular
    ///     <see cref="Exception" />, or at any level of nesting within an <see cref="AggregateException" />.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public PolicyExpression OrInner<TException>() where TException : Exception
    {
        _match.Or(new InnerMatch(new TypeMatch<TException>()));
        return this;
    }

    /// <summary>
    ///     Specifies an additional type of exception that this policy can handle, with additional filters on this exception
    ///     type, if
    ///     found as an InnerException of a regular <see cref="Exception" />, or at any level of nesting within an
    ///     <see cref="AggregateException" />.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <param name="description">Optional description of the filter for diagnostic purposes</param>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public PolicyExpression OrInner<TException>(Func<TException, bool> exceptionPredicate, string description = "User supplied filter")
        where TException : Exception
    {
        _match = _match.Or(new InnerMatch(new UserSupplied<TException>(exceptionPredicate, description)));
        return this;
    }

    /// <summary>
    ///     Immediately move the message to the error queue when the exception
    ///     caught matches this criteria
    /// </summary>
    public IAdditionalActions MoveToErrorQueue()
    {
        return new FailureActions(_match, _parent).MoveToErrorQueue();
    }

    /// <summary>
    ///     Requeue the message back to the incoming transport, with the message being
    ///     dead lettered when the maximum number of attempts is reached
    /// </summary>
    /// <param name="maxAttempts">The maximum number of attempts to process the message. The default is 3</param>
    public IAdditionalActions Requeue(int maxAttempts = 3)
    {
        return new FailureActions(_match, _parent).Requeue(maxAttempts);
    }

    /// <summary>
    ///     Discard the message without any further attempt to process the message
    /// </summary>
    public IAdditionalActions Discard()
    {
        return new FailureActions(_match, _parent).Discard();
    }

    /// <summary>
    ///     Schedule the message for additional attempts with a delay. Use this
    ///     method to effect an "exponential backoff" policy
    /// </summary>
    /// <param name="delays"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public IAdditionalActions ScheduleRetry(params TimeSpan[] delays)
    {
        return new FailureActions(_match, _parent).ScheduleRetry(delays);
    }

    /// <summary>
    ///     Retry the message a maximum number of attempts without any delay
    ///     or moving the message back to the original queue
    /// </summary>
    /// <param name="maxAttempts"></param>
    public IAdditionalActions RetryNow(int maxAttempts = 3)
    {
        return new FailureActions(_match, _parent).RetryNow(3);
    }

    /// <summary>
    /// Retry message failures a define number of times with user-specified cooldown times
    /// between events. This allows for "exponential backoff" strategies
    /// </summary>
    /// <param name="delays"></param>
    /// <param name="maxAttempts"></param>
    public IAdditionalActions RetryWithCooldown(params TimeSpan[] delays)
    {
        return new FailureActions(_match, _parent).RetryWithCooldown(delays);
    }
}
