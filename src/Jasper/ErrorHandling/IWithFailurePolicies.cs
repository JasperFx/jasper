using Jasper.ErrorHandling.New;

namespace Jasper.ErrorHandling;

public interface IWithFailurePolicies
{
    /// <summary>
    ///     Collection of Error handling policies for exception handling during the execution of a message
    /// </summary>
    FailureRuleCollection Failures { get; }
}
