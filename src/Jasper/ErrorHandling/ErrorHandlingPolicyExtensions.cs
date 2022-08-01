using System;
using Baseline;

namespace Jasper.ErrorHandling;

public static class ErrorHandlingPolicyExtensions
{
    /// <summary>
    ///     Specifies the type of exception that this policy can handle.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance.</returns>
    public static PolicyExpression OnException<TException>(this IHasFailurePolicies policies) where TException : Exception
    {
        return new PolicyExpression(policies.Failures, new TypeMatch<TException>());
    }

    /// <summary>
    ///     Specifies the type of exception that this policy can handle with additional filters on this exception type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="policies"></param>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <param name="description">Optional description of this exception filter strictly for diagnostics</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public static PolicyExpression OnException(this IHasFailurePolicies policies,
        Func<Exception, bool> exceptionPredicate, string description = "User supplied")
    {
        return new PolicyExpression(policies.Failures, new UserSupplied(exceptionPredicate, description));
    }

    /// <summary>
    ///     Specifies the type of exception that this policy can handle with additional filters on this exception type.
    /// </summary>
    /// <param name="policies"></param>
    /// <param name="exceptionType">An exception type to match against</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public static PolicyExpression OnExceptionOfType(this IHasFailurePolicies policies, Type exceptionType)
    {
        // TODO -- switch to returning TypeMatch later
        return policies.OnException(e => e.GetType().CanBeCastTo(exceptionType));
    }


    /// <summary>
    ///     Specifies the type of exception that this policy can handle with additional filters on this exception type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="policies"></param>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <param name="description">Optional description of this exception filter strictly for diagnostics</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public static PolicyExpression OnException<TException>(this IHasFailurePolicies policies,
        Func<TException, bool> exceptionPredicate, string description = "User supplied")
        where TException : Exception
    {
        return new PolicyExpression(policies.Failures, new UserSupplied<TException>(exceptionPredicate, description));
    }

    /// <summary>
    ///     Specifies the type of exception that this policy can handle if found as an InnerException of a regular
    ///     <see cref="Exception" />, or at any level of nesting within an <see cref="AggregateException" />.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public static PolicyExpression HandleInner<TException>(this IHasFailurePolicies policies) where TException : Exception
    {
        return new PolicyExpression(policies.Failures, new InnerMatch(new TypeMatch<TException>()));
    }

    /// <summary>
    ///     Specifies the type of exception that this policy can handle, with additional filters on this exception type, if
    ///     found as an InnerException of a regular <see cref="Exception" />, or at any level of nesting within an
    ///     <see cref="AggregateException" />.
    /// </summary>
    /// <param name="description">Optional description of this exception filter strictly for diagnostics</param>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public static PolicyExpression HandleInner<TException>(this IHasFailurePolicies policies,
        Func<TException, bool> exceptionPredicate, string description = "User supplied filter")
        where TException : Exception
    {
        return new PolicyExpression(policies.Failures, new InnerMatch(new UserSupplied<TException>(exceptionPredicate, description)));
    }
}
