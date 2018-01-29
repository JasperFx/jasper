using System;
using System.Linq;
using System.Linq.Expressions;
using Baseline.Reflection;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime.Invocation;
using Shouldly;

namespace Jasper.Testing.Bus
{
    public static class HandlerChainSpecificationExtensions
    {
        public static void ShouldHaveHandler<T>(this HandlerChain chain, Expression<Action<T>> expression)
        {
            ShouldBeNullExtensions.ShouldNotBeNull(chain);

            var method = ReflectionHelper.GetMethod(expression);
            ShouldBeBooleanExtensions.ShouldBeTrue(chain.Handlers.Any(x => x.Method.Name == method.Name));
        }

        public static void ShouldHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            ShouldBeNullExtensions.ShouldNotBeNull(chain);
            ShouldBeBooleanExtensions.ShouldBeTrue(chain.Handlers.Any(x => x.Method.Name == methodName && x.HandlerType == typeof(T)));
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, Expression<Action<T>> expression)
        {
            if (chain == null) return;

            var method = ReflectionHelper.GetMethod(expression);
            ShouldBeBooleanExtensions.ShouldBeFalse(chain.Handlers.Any(x => x.Method.Name == method.Name && x.HandlerType == typeof(T)));
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            chain?.Handlers.Any(x => x.Method.Name == methodName).ShouldBeFalse();
        }

        public static void ShouldBeWrappedWith<T>(this HandlerChain chain) where T : Frame
        {
            ShouldBeNullExtensions.ShouldNotBeNull(chain);
            ShouldBeBooleanExtensions.ShouldBeTrue(chain.Middleware.OfType<T>().Any());
        }

        public static void ShouldHandleExceptionWith<TEx, TContinuation>(this HandlerChain chain)
            where TEx : Exception
            where TContinuation : IContinuation
        {
            ShouldBeBooleanExtensions.ShouldBeTrue(chain.ErrorHandlers.OfType<ErrorHandler>()
                    .Where(x => x.Conditions.Count() == 1 && x.Conditions.Single() is ExceptionTypeMatch<TEx>)
                    .SelectMany(x => x.Sources)
                    .OfType<ContinuationSource>()
                    .Any(x => x.Continuation is TContinuation));
        }

        public static void ShouldMoveToErrorQueue<T>(this HandlerChain chain) where T : Exception
        {
            ShouldBeBooleanExtensions.ShouldBeTrue(chain.ErrorHandlers.OfType<ErrorHandler>()
                    .Where(x => x.Conditions.Count() == 1 && x.Conditions.Single() is ExceptionTypeMatch<T>)
                    .SelectMany(x => x.Sources)
                    .OfType<MoveToErrorQueueHandler<T>>()
                    .Any());
        }
    }
}
