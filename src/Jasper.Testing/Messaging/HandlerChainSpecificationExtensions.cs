using System;
using System.Linq;
using System.Linq.Expressions;
using Baseline.Reflection;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime.Invocation;
using Lamar.Codegen.Frames;
using Shouldly;

namespace Jasper.Testing.Messaging
{
    public static class HandlerChainSpecificationExtensions
    {
        public static void ShouldHaveHandler<T>(this HandlerChain chain, Expression<Action<T>> expression)
        {
            ShouldBeNullExtensions.ShouldNotBeNull(chain);

            var method = ReflectionHelper.GetMethod(expression);
            chain.Handlers.Any(x => x.Method.Name == method.Name).ShouldBeTrue();
        }

        public static void ShouldHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            ShouldBeNullExtensions.ShouldNotBeNull(chain);
            chain.Handlers.Any(x => x.Method.Name == methodName && x.HandlerType == typeof(T)).ShouldBeTrue();
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, Expression<Action<T>> expression)
        {
            if (chain == null) return;

            var method = ReflectionHelper.GetMethod(expression);
            chain.Handlers.Any(x => x.Method == method).ShouldBeFalse();
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            chain?.Handlers.Any(x => x.Method.Name == methodName).ShouldBeFalse();
        }

        public static void ShouldBeWrappedWith<T>(this HandlerChain chain) where T : Frame
        {
            ShouldBeNullExtensions.ShouldNotBeNull(chain);
            chain.Middleware.OfType<T>().Any().ShouldBeTrue();
        }

        public static void ShouldHandleExceptionWith<TEx, TContinuation>(this HandlerChain chain)
            where TEx : Exception
            where TContinuation : IContinuation
        {
            chain.ErrorHandlers.OfType<ErrorHandler>()
                .Where(x => x.Conditions.Count() == 1 && x.Conditions.Single() is ExceptionTypeMatch<TEx>)
                .SelectMany(x => x.Sources)
                .OfType<ContinuationSource>()
                .Any(x => x.Continuation is TContinuation).ShouldBeTrue();
        }

        public static void ShouldMoveToErrorQueue<T>(this HandlerChain chain) where T : Exception
        {
            chain.ErrorHandlers.OfType<ErrorHandler>()
                .Where(x => x.Conditions.Count() == 1 && x.Conditions.Single() is ExceptionTypeMatch<T>)
                .SelectMany(x => x.Sources)
                .OfType<MoveToErrorQueueHandler<T>>()
                .Any().ShouldBeTrue();
        }
    }
}
