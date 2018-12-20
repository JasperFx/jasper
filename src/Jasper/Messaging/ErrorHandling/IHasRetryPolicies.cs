namespace Jasper.Messaging.ErrorHandling
{
    public interface IHasRetryPolicies
    {
        /// <summary>
        ///     Collection of Polly policies for exception handling during the execution of a message
        /// </summary>
        RetryPolicyCollection Retries { get; set; }
    }
}
