using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using JasperBus.Model;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using Shouldly;
using StructureMap;
using Xunit;

namespace JasperBus.Tests.Compilation
{
    [Collection("compilation")]
    public abstract class CompilationContext<T>
    {
        private Lazy<IContainer> _container;
        public readonly ServiceRegistry services = new ServiceRegistry();

        protected Lazy<Dictionary<Type, MessageHandler>> _handlers;

        private readonly Lazy<string> _code;
        protected Envelope theEnvelope;

        protected Lazy<MessageHandlerGraph> _graph;

        public CompilationContext()
        {
            _container = new Lazy<IContainer>(() => new Container(services));

            _graph = new Lazy<MessageHandlerGraph>(() =>
            {
                var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
                var container = _container.Value;
                config.Sources.Add(new StructureMapServices(container));

                config.Assemblies.Add(typeof(IContainer).GetTypeInfo().Assembly);
                config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

                var graph = new MessageHandlerGraph(config);

                var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(x => x.DeclaringType != typeof(object) && x != null && x.GetParameters().Any() && !x.IsSpecialName);

                foreach (var method in methods)
                {
                    graph.Add(new HandlerCall(typeof(T), method));
                }

                return graph;
            });



            _code = new Lazy<string>(() => Graph.GenerateCode());

            _handlers = new Lazy<Dictionary<Type, MessageHandler>>(() =>
            {
                var handlers = Graph.CompileAndBuildAll(_container.Value);
                var dict = new Dictionary<Type, MessageHandler>();
                foreach (var handler in handlers)
                {
                    dict.Add(handler.Chain.MessageType, handler);
                }

                return dict;
            });
        }

        public MessageHandlerGraph Graph => _graph.Value;

        [Fact]
        public void can_compile_all()
        {
            AllHandlersCompileSuccessfully();
        }

        public string theCode => _code.Value;

        public void AllHandlersCompileSuccessfully()
        {
            _handlers.Value.Count.ShouldBeGreaterThan(0);
        }

        public MessageHandler HandlerFor<TMessage>()
        {
            return _handlers.Value[typeof(TMessage)];
        }

        public async Task<IInvocationContext> Execute<TMessage>(TMessage message)
        {
            var handler = HandlerFor<TMessage>();
            theEnvelope = Envelope.ForMessage(message);
            var context = new InvocationContext(theEnvelope, handler.Chain );

            await handler.Handle(context);

            return context;
        }
    }
}