using System.Collections.Generic;

namespace Jasper.ErrorHandling.New;

public class FailureRuleCollection
{
    private readonly List<FailureRule> _rules = new();

    /// <summary>
    ///     Maximum number of attempts allowed for this message type
    /// </summary>
    public int? MaximumAttempts { get; set; }

    internal IEnumerable<FailureRule> CombineRules(FailureRuleCollection parent)
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

    internal static FailureRule BuildRequeueRuleForMaximumAttempts(int maximumAttempts)
    {
        var rule = new FailureRule(new AlwaysMatches());
        for (int i = 0; i < maximumAttempts - 1; i++)
        {
            rule.AddSlot(RequeueContinuation.Instance);
        }

        return rule;
    }

    internal void Add(FailureRule rule)
    {
        _rules.Add(rule);
    }
}
