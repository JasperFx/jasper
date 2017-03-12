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
            with(new JasperBusRegistry());
            _graph = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(JasperBusRegistry registry)
        {
            registry.Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Runtime = JasperRuntime.For(registry);


            _graph = Runtime.Container.GetInstance<HandlerGraph>();
        }

        protected void with(Action<JasperBusRegistry> configuration)
        {
            var registry = new JasperBusRegistry();


            configuration(registry);

            with(registry);
        }

        protected void with<T>() where T : JasperBusRegistry, new()
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
            chain.ShouldNotBeNull();

            var method = ReflectionHelper.GetMethod(expression);
            chain.Handlers.Any(x => x.Method.Name == method.Name).ShouldBeTrue();
        }

        public static void ShouldHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            chain.ShouldNotBeNull();
            chain.Handlers.Any(x => x.Method.Name == methodName && x.HandlerType == typeof(T)).ShouldBeTrue();
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, Expression<Action<T>> expression)
        {
            if (chain == null) return;

            var method = ReflectionHelper.GetMethod(expression);
            chain.Handlers.Any(x => x.Method.Name == method.Name && x.HandlerType == typeof(T)).ShouldBeFalse();
        }

        public static void ShouldNotHaveHandler<T>(this HandlerChain chain, string methodName)
        {
            chain?.Handlers.Any(x => x.Method.Name == methodName).ShouldBeFalse();
        }
    }
}