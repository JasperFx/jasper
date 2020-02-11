using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Runtime;
using Polly;

namespace Jasper.ErrorHandling
{

    public class PolicyExpression
    {
        private readonly RetryPolicyCollection _parent;
        private PolicyBuilder<IContinuation> _builder;

        internal PolicyExpression(RetryPolicyCollection parent, PolicyBuilder<IContinuation> builder)
        {
            _parent = parent;
            _builder = builder;
        }



        /// <summary>
        /// Specifies an additional type of exception that this policy can handle.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyExpression Or<TException>() where TException : Exception
        {
            _builder = _builder.Or<TException>();
            return this;
        }

        /// <summary>
        /// Specifies an additional type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyExpression Or(Func<Exception, bool> exceptionPredicate)
        {
            _builder = _builder.Or(exceptionPredicate);
            return this;
        }


        /// <summary>
        /// Specifies an additional type of exception that this policy can handle with additional filters on this exception type.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
        /// <returns>The PolicyBuilder instance.</returns>
        public PolicyExpression Or<TException>(Func<TException, bool> exceptionPredicate)
            where TException : Exception
        {
            _builder = _builder.Or(exceptionPredicate);
            return this;
        }

        /// <summary>
        ///     Specifies an additional type of exception that this policy can handle if found as an InnerException of a regular
        ///     <see cref="Exception" />, or at any level of nesting within an <see cref="AggregateException" />.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
        public PolicyExpression OrInner<TException>() where TException : Exception
        {
            _builder = _builder.OrInner<TException>();
            return this;
        }

        /// <summary>
        ///     Specifies an additional type of exception that this policy can handle, with additional filters on this exception type, if
        ///     found as an InnerException of a regular <see cref="Exception" />, or at any level of nesting within an
        ///     <see cref="AggregateException" />.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to handle.</typeparam>
        /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
        public PolicyExpression OrInner<TException>(Func<TException, bool> exceptionPredicate)
            where TException : Exception
        {
            _builder = _builder.OrInner(exceptionPredicate);
            return this;
        }

        /// <summary>
        /// Use a custom IContinuation factory for matching exceptions
        /// </summary>
        /// <param name="continuationSource"></param>
        public void With(Func<Envelope, Exception, IContinuation> continuationSource)
        {
            var policy = _builder.FallbackAsync((result, context, token) =>
            {
                var envelope = context.MessageContext().Envelope;
                var continuation = continuationSource(envelope, result.Exception);

                return Task.FromResult(continuation);
            }, (result, context) => Task.CompletedTask);

            _parent.Add(policy);
        }

        /// <summary>
        /// Immediately move the message to the error queue when the exception
        /// caught matches this criteria
        /// </summary>
        public void MoveToErrorQueue()
        {
            With((e, ex) => new MoveToErrorQueue(ex));
        }

        /// <summary>
        /// Requeue the message back to the incoming transport, with the message being
        /// dead lettered when the maximum number of attempts is reached
        /// </summary>
        /// <param name="maxAttempts">The maximum number of attempts to process the message. The default is 3</param>
        public void Requeue(int maxAttempts = 3)
        {
            With((e, ex) => e.Attempts < maxAttempts
                ? (IContinuation) RequeueContinuation.Instance
                : new MoveToErrorQueue(ex));
        }

        /// <summary>
        /// Schedule the message for additional attempts with a delay. Use this
        /// method to effect an "exponential backoff" policy
        /// </summary>
        /// <param name="delays"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RetryLater(params TimeSpan[] delays)
        {
            if (!delays.Any())
            {
                throw new InvalidOperationException("You must specify at least one delay time");
            }

            With((e, ex) => e.Attempts < delays.Length
                ? (IContinuation) new ScheduledRetryContinuation(delays[e.Attempts - 1])
                : new MoveToErrorQueue(ex));
        }

        /// <summary>
        /// Retry the message a maximum number of attempts without any delay
        /// or moving the message back to the original queue
        /// </summary>
        /// <param name="maxAttempts"></param>
        public void RetryNow(int maxAttempts = 3)
        {
            With((e, ex) => e.Attempts < maxAttempts
                ? (IContinuation) RetryNowContinuation.Instance
                : new MoveToErrorQueue(ex));
        }

    }

}
