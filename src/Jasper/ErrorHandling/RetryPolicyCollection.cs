using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jasper.Runtime;
using Polly;

namespace Jasper.ErrorHandling;

public class RetryPolicyCollection : IEnumerable<IAsyncPolicy<IContinuation>>
{
    private readonly IList<IAsyncPolicy<IContinuation>> _policies = new List<IAsyncPolicy<IContinuation>>();

    /// <summary>
    ///     Maximum number of attempts allowed for this message type
    /// </summary>
    public int? MaximumAttempts { get; set; }


    public IEnumerator<IAsyncPolicy<IContinuation>> GetEnumerator()
    {
        return _policies.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(IAsyncPolicy<IContinuation> policy)
    {
        _policies.Add(policy);
    }


    public void Clear()
    {
        _policies.Clear();
    }

    private IEnumerable<IAsyncPolicy<IContinuation>> combine(RetryPolicyCollection parent)
    {
        foreach (var policy in _policies) yield return policy;

        if (MaximumAttempts.HasValue)
        {
            yield return Policy<IContinuation>.Handle<Exception>().Requeue(MaximumAttempts.Value);
        }

        foreach (var policy in parent._policies) yield return policy;

        if (parent.MaximumAttempts.HasValue)
        {
            yield return Policy<IContinuation>.Handle<Exception>().Requeue(parent.MaximumAttempts.Value);
        }
    }

    internal IAsyncPolicy<IContinuation>? BuildPolicy(RetryPolicyCollection parent)
    {
        var policies = combine(parent).Reverse().ToArray();

        if (policies.Length != 0)
        {
            return policies.Length == 1
                ? policies[0]
                : Policy.WrapAsync(policies);
        }

        return null;
    }
}
