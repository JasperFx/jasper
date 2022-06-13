using System;
using System.Collections;
using System.Collections.Generic;

namespace Jasper.ErrorHandling;

public class RetryPolicyCollection : IEnumerable<ExceptionRule>
{
    private readonly List<ExceptionRule> _rules = new();

    /// <summary>
    ///     Maximum number of attempts allowed for this message type
    /// </summary>
    public int? MaximumAttempts { get; set; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<ExceptionRule> GetEnumerator()
    {
        return _rules.GetEnumerator();
    }

    public void Clear()
    {
        _rules.Clear();
    }

    internal IEnumerable<ExceptionRule> CombineRules(RetryPolicyCollection parent)
    {
        foreach (var rule in _rules) yield return rule;

        if (MaximumAttempts.HasValue)
        {
            yield return BuildRequeueRuleForMaximumAttempts(MaximumAttempts.Value);
        }

        foreach (var rule in parent._rules) yield return rule;

        if (parent.MaximumAttempts.HasValue)
        {
            yield return BuildRequeueRuleForMaximumAttempts(parent.MaximumAttempts.Value);
        }
    }

    public static ExceptionRule BuildRequeueRuleForMaximumAttempts(int maximumAttempts)
    {
        return new ExceptionRule(e => true, (e, ex) =>
        {
            if (e.Attempts < maximumAttempts)
            {
                return RequeueContinuation.Instance;
            }

            return new MoveToErrorQueue(ex);
        });
    }

    public void Add(ExceptionRule rule)
    {
        _rules.Add(rule);
    }
}
