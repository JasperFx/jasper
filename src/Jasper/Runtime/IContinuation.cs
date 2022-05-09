using System;
using System.Threading.Tasks;

namespace Jasper.Runtime;

#region sample_IContinuation

/// <summary>
///     Represents an action to take after processing a message
/// </summary>
public interface IContinuation
{
    /// <summary>
    ///     Post-message handling action
    /// </summary>
    /// <param name="execution"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    ValueTask ExecuteAsync(IExecutionContext execution, DateTimeOffset now);
}

#endregion
