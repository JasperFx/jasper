using System;
using Polly;

namespace Jasper.ErrorHandling
{
    public interface IHasRetryPolicies
    {
        /// <summary>
        ///     Collection of Polly policies for exception handling during the execution of a message
        /// </summary>
        RetryPolicyCollection Retries { get; set; }
    }

    public static class RetryPolicyExtensions
    {
        public static void MoveToDeadLetterQueueOn<T>(this IHasRetryPolicies policies, Func<T, bool> filter = null) where T : Exception
        {
            if (filter == null)
            {
                policies.Retries.Add(x => x.Handle<T>().MoveToErrorQueue());
            }
            else
            {
                policies.Retries.Add(x => x.Handle(filter).MoveToErrorQueue());
            }
        }

        public static void RetryOn<T>(this IHasRetryPolicies policies, Func<T, bool> filter = null) where T : Exception
        {
            if (filter == null)
            {
                policies.Retries.Add(x => x.Handle<T>().RetryAsync());
            }
            else
            {
                policies.Retries.Add(x => x.Handle(filter).RetryAsync());
            }
        }

        public static void RequeueOn<T>(this IHasRetryPolicies policies, Func<T, bool> filter = null) where T : Exception
        {
            if (filter == null)
            {
                policies.Retries.Add(x => x.Handle<T>().Requeue());
            }
            else
            {
                policies.Retries.Add(x => x.Handle(filter).Requeue());
            }
        }
    }
}
