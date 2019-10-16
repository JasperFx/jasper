﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Baseline.Reflection;
using Jasper.Messaging.Model;
using LamarCodeGeneration.Frames;
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
            chain.ShouldNotBeNull();
            chain.Middleware.OfType<T>().Any().ShouldBeTrue();
        }


    }
}
