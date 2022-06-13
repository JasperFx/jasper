namespace Jasper.ErrorHandling;

public interface IHasRetryPolicies
{
    /// <summary>
    ///     Collection of Error handling policies for exception handling during the execution of a message
    /// </summary>
    RetryPolicyCollection Retries { get; }
}
