using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime.Invocation;
using Polly;
using Polly.Retry;

namespace Jasper.Messaging.ErrorHandling
{
    public class RetryPolicyCollection : IEnumerable<IAsyncPolicy<IContinuation>>
    {
        private readonly IList<IAsyncPolicy<IContinuation>> _policies = new List<IAsyncPolicy<IContinuation>>();

        /// <summary>
        /// Maximum number of attempts allowed for this message type
        /// </summary>
        public int? MaximumAttempts { get; set; }

        public void Add(IAsyncPolicy<IContinuation> policy)
        {
            _policies.Add(policy);
        }

        public static RetryPolicyCollection operator +(RetryPolicyCollection collection, IAsyncPolicy<IContinuation> policy)
        {
            collection.Add(policy);
            return collection;
        }

        /// <summary>
        /// Add a single Polly error handler
        /// </summary>
        /// <param name="configure"></param>
        public void Add(Func<PolicyBuilder, Policy<IContinuation>> configure)
        {
            var builder = new PolicyBuilder();
            var policy = configure(builder);
            Add(policy);
        }

        /// <summary>
        /// Add a single Polly error handler
        /// </summary>
        /// <param name="configure"></param>
        public void AddMany(Func<PolicyBuilder, IEnumerable<Policy<IContinuation>>> configure)
        {
            var builder = new PolicyBuilder();
            var policies = configure(builder);
            _policies.AddRange(policies);
        }


        public class PolicyBuilder
        {
            /// <summary>
            ///     Specifies the type of exception that this policy can handle.
            /// </summary>
            /// <typeparam name="TException">The type of the exception to handle.</typeparam>
            /// <returns>The PolicyBuilder instance.</returns>
            public PolicyBuilder<IContinuation> Handle<TException>() where TException : Exception
            {
                return Policy<IContinuation>.Handle<TException>();
            }

            /// <summary>
            ///     Specifies the type of exception that this policy can handle with additional filters on this exception type.
            /// </summary>
            /// <typeparam name="TException">The type of the exception.</typeparam>
            /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
            /// <returns>The PolicyBuilder instance.</returns>
            public PolicyBuilder<IContinuation> Handle<TException>(Func<TException, bool> exceptionPredicate)
                where TException : Exception
            {
                return Policy<IContinuation>.Handle(exceptionPredicate);
            }

            /// <summary>
            ///     Specifies the type of exception that this policy can handle if found as an InnerException of a regular
            ///     <see cref="Exception" />, or at any level of nesting within an <see cref="AggregateException" />.
            /// </summary>
            /// <typeparam name="TException">The type of the exception to handle.</typeparam>
            /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
            public PolicyBuilder<IContinuation> HandleInner<TException>() where TException : Exception
            {
                return Policy<IContinuation>.HandleInner<TException>();
            }

            /// <summary>
            ///     Specifies the type of exception that this policy can handle, with additional filters on this exception type, if
            ///     found as an InnerException of a regular <see cref="Exception" />, or at any level of nesting within an
            ///     <see cref="AggregateException" />.
            /// </summary>
            /// <typeparam name="TException">The type of the exception to handle.</typeparam>
            /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
            public PolicyBuilder<IContinuation> HandleInner<TException>(Func<TException, bool> exceptionPredicate)
                where TException : Exception
            {
                return Policy<IContinuation>.HandleInner(exceptionPredicate);
            }
        }

        public IEnumerator<IAsyncPolicy<IContinuation>> GetEnumerator()
        {
            return _policies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddMany(IEnumerable<IAsyncPolicy<IContinuation>> policies)
        {
            _policies.AddRange(policies);
        }

        public void Clear()
        {
            _policies.Clear();
        }

        private IEnumerable<IAsyncPolicy<IContinuation>> combine(RetryPolicyCollection parent)
        {
            foreach (var policy in _policies)
            {
                yield return policy;
            }

            if (MaximumAttempts.HasValue) yield return Policy<IContinuation>.Handle<Exception>().Requeue(MaximumAttempts.Value);

            foreach (var policy in parent._policies)
            {
                yield return policy;
            }

            if (parent.MaximumAttempts.HasValue) yield return Policy<IContinuation>.Handle<Exception>().Requeue(parent.MaximumAttempts.Value);
        }

        public IAsyncPolicy<IContinuation> BuildPolicy(RetryPolicyCollection parent)
        {
            var policies = combine(parent).Reverse().ToArray();

            if (policies.Length != 0)
                return policies.Length == 1
                    ? policies[0]
                    : Policy.WrapAsync(policies);

            return null;
        }
    }
}
