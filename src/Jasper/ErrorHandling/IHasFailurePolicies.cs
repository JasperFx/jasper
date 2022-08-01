using Jasper.ErrorHandling.New;

namespace Jasper.ErrorHandling;

public interface IHasFailurePolicies
{
    /// <summary>
    ///     Collection of Error handling policies for exception handling during the execution of a message
    /// </summary>
    FailureRuleCollection Failures { get; }
}
