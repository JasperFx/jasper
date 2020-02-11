using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime;
using Polly;

namespace Jasper.ErrorHandling
{
    public static class JasperPollyExtensions
    {
        public const string ContextKey = "context";

        internal static IAsyncPolicy<IContinuation> Requeue(this PolicyBuilder<IContinuation> builder, int maxAttempts = 3)
        {
            return builder.FallbackAsync((result, context, token) =>
            {
                var envelope = context.MessageContext().Envelope;

                var continuation = envelope.Attempts < maxAttempts
                    ? (IContinuation) RequeueContinuation.Instance
                    : new MoveToErrorQueue(result.Exception);

                return Task.FromResult(continuation);
            }, (result, context) => Task.CompletedTask);
        }

        internal static void Store(this Context context, IMessageContext messageContext)
        {
            context.Add(ContextKey, messageContext);
        }

        internal static IMessageContext MessageContext(this Context context)
        {
            return context[ContextKey].As<IMessageContext>();
        }



        /// <summary>
        ///     Specifies the type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyExpression OnException<TException>(this IHasRetryPolicies policies) where TException : Exception
        {
            var builder = Policy<IContinuation>.Handle<TException>();
            return new PolicyExpression(policies.Retries, builder);
        }

        /// <summary>
        ///     Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="policies"></param>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyExpression OnException(this IHasRetryPolicies policies, Func<Exception, bool> exceptionPredicate)
        {
            var builder = Policy<IContinuation>.Handle(exceptionPredicate);
            return new PolicyExpression(policies.Retries, builder);
        }

        /// <summary>
        ///     Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <param name="policies"></param>
        /// <param name="exceptionType">An exception type to match against</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyExpression OnExceptionOfType(this IHasRetryPolicies policies, Type exceptionType)
        {
            return policies.OnException(e => e.GetType().CanBeCastTo(exceptionType));
        }


        /// <summary>
        ///     Specifies the type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="policies"></param>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public static PolicyExpression OnException<TException>(this IHasRetryPolicies policies, Func<TException, bool> exceptionPredicate)
            where TException : Exception
        {
            var builder = Policy<IContinuation>.Handle(exceptionPredicate);
            return new PolicyExpression(policies.Retries, builder);
        }

        /// <summary>
        ///     Specifies the type of exception that this policy can handle if found as an InnerException of a regular
        ///     <see cref="Exception" />, or at any level of nesting within an <see cref="AggregateException" />.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
        public static PolicyExpression HandleInner<TException>(this IHasRetryPolicies policies) where TException : Exception
        {
            var builder = Policy<IContinuation>.HandleInner<TException>();
            return new PolicyExpression(policies.Retries, builder);
        }

        /// <summary>
        ///     Specifies the type of exception that this policy can handle, with additional filters on this exception type, if
        ///     found as an InnerException of a regular <see cref="Exception" />, or at any level of nesting within an
        ///     <see cref="AggregateException" />.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
        public static PolicyExpression HandleInner<TException>(this IHasRetryPolicies policies, Func<TException, bool> exceptionPredicate)
            where TException : Exception
        {
            var builder = Policy<IContinuation>.HandleInner(exceptionPredicate);
            return new PolicyExpression(policies.Retries, builder);
        }





    }
}
