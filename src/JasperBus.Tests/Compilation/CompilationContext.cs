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

namespace JasperBus.Tests.Compilation
{
    public class CompilationContext<T>
    {
        private Lazy<IContainer> _container;
        protected readonly MessageHandlerGraph Graph;
        public readonly ServiceRegistry services = new ServiceRegistry();

        protected Lazy<Dictionary<Type, MessageHandler>> _handlers;

        private readonly Lazy<string> _code;

        public CompilationContext()
        {
            _container = new Lazy<IContainer>(() => new Container(services));

            var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
            var container = _container.Value;
            config.Sources.Add(new StructureMapServices(container));

            config.Assemblies.Add(typeof(IContainer).GetTypeInfo().Assembly);
            config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

            Graph = new MessageHandlerGraph(config);

            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => x.DeclaringType != typeof(object) && x != null);

            foreach (var method in methods)
            {
                Graph.Add(new HandlerCall(typeof(T), method));
            }

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

        public string theCode => _code.Value;

        public void AllHandlersCompileSuccessfully()
        {
            _handlers.Value.Count.ShouldBeGreaterThan(0);
        }

        public MessageHandler HandlerFor<T>()
        {
            return _handlers.Value[typeof(T)];
        }

        public async Task<IInvocationContext> Execute<T>(T message)
        {
            var handler = HandlerFor<T>();
            var context = new InvocationContext(Envelope.ForMessage(message), handler.Chain );

            await handler.Handle(context);

            return context;
        }
    }
}