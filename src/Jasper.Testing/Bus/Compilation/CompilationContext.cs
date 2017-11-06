using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Configuration;
using Jasper.Internals;
using Jasper.Internals.Codegen;
using Jasper.Internals.Codegen.ServiceLocation;
using Jasper.Internals.IoC;
using Shouldly;
using StructureMap;
using Xunit;

namespace Jasper.Testing.Bus.Compilation
{
    [Collection("compilation")]
    public abstract class CompilationContext<T>
    {
        private Lazy<IContainer> _container;
        public readonly ServiceRegistry services = new ServiceRegistry();

        protected Lazy<Dictionary<Type, MessageHandler>> _handlers;

        private readonly Lazy<string> _code;
        protected Envelope theEnvelope;

        protected Lazy<HandlerGraph> _graph;
        private GenerationRules rules;

        public CompilationContext()
        {
            _container = new Lazy<IContainer>(() =>
            {
                var registry = new Registry();
                registry.Populate(services);
                return new Container(registry);
            });


            _graph = new Lazy<HandlerGraph>(() =>
            {
                rules = new GenerationRules("Jasper.Testing.Codegen.Generated");
                rules.Sources.Add(new ServiceGraph(services));
                rules.Sources.Add(new NoArgConcreteCreator());

                rules.Assemblies.Add(typeof(IContainer).GetTypeInfo().Assembly);
                rules.Assemblies.Add(GetType().GetTypeInfo().Assembly);


                var graph = new HandlerGraph();

                var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(x => x.DeclaringType != typeof(object) && x != null && x.GetParameters().Any() && !x.IsSpecialName);

                foreach (var method in methods)
                {
                    graph.Add(new HandlerCall(typeof(T), method));
                }

                return graph;
            });



            _code = new Lazy<string>(() => Graph.GenerateCode(rules));

            _handlers = new Lazy<Dictionary<Type, MessageHandler>>(() =>
            {
                var handlers = Graph.CompileAndBuildAll(rules, _container.Value.GetInstance);
                var dict = new Dictionary<Type, MessageHandler>();
                foreach (var handler in handlers)
                {
                    dict.Add(handler.Chain.MessageType, handler);
                }

                return dict;
            });
        }

        public IContainer Container => _container.Value;

        public HandlerGraph Graph => _graph.Value;

        [Fact]
        public void can_compile_all()
        {
            AllHandlersCompileSuccessfully();
        }

        public string theCode => _code.Value;

        public void AllHandlersCompileSuccessfully()
        {
            ShouldBeTestExtensions.ShouldBeGreaterThan(_handlers.Value.Count, 0);
        }

        public MessageHandler HandlerFor<TMessage>()
        {
            return _handlers.Value[typeof(TMessage)];
        }

        public async Task<IInvocationContext> Execute<TMessage>(TMessage message)
        {
            var handler = HandlerFor<TMessage>();
            theEnvelope = new Envelope(message);
            var context = new EnvelopeContext(null,theEnvelope, null);

            await handler.Handle(context);

            return context;
        }
    }
}
