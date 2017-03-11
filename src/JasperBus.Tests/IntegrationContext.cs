using System;
using System.Linq;
using System.Linq.Expressions;
using Baseline.Reflection;
using Jasper;
using JasperBus.Model;
using Shouldly;

namespace JasperBus.Tests
{
    public class IntegrationContext : IDisposable
    {
        public JasperRuntime Runtime { get; private set; }
        private HandlerGraph _graph;

        protected void withAllDefaults()
        {
            Runtime = JasperRuntime.Basic();
            _graph = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(JasperRegistry registry)
        {
            Runtime = JasperRuntime.For(registry);
            _graph = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(Action<JasperRegistry> configuration)
        {
            var registry = new JasperRegistry();
            configuration(registry);

            with(registry);
        }

        protected void with<T>() where T : JasperRegistry, new()
        {
            var registry = new T();
            with(registry);
        }

        public void Dispose()
        {
            Runtime.Dispose();
        }

        protected HandlerChain chainFor<T>()
        {
            return _graph.ChainFor<T>();
        }
    }

    public static class HandlerChainSpecificationExtensions
    {
        public static void ShouldHaveHandler<T>(this HandlerChain chain, Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            chain.Handlers.Any(x => x.Method.Name == method.Name).ShouldBeTrue();
        }

        public static void ShouldHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            chain.Handlers.Any(x => x.Method.Name == methodName).ShouldBeTrue();
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);
            chain.Handlers.Any(x => x.Method.Name == method.Name).ShouldBeFalse();
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            chain.Handlers.Any(x => x.Method.Name == methodName).ShouldBeFalse();
        }
    }
}