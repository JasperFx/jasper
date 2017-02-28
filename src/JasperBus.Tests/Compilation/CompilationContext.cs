using System;
using System.Linq;
using System.Reflection;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using JasperBus.Model;
using StructureMap;

namespace JasperBus.Tests.Compilation
{
    public class CompilationContext<T>
    {
        private Lazy<IContainer> _container;
        protected readonly HandlerGraph Graph;
        public readonly ServiceRegistry services = new ServiceRegistry();

        private readonly Lazy<string> _code;

        public CompilationContext()
        {
            _container = new Lazy<IContainer>(() => new Container(services));

            var config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
            var container = _container.Value;
            config.Sources.Add(new StructureMapServices(container));

            config.Assemblies.Add(typeof(IContainer).GetTypeInfo().Assembly);
            config.Assemblies.Add(GetType().GetTypeInfo().Assembly);

            Graph = new HandlerGraph(config);

            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => x.DeclaringType != typeof(object) && x != null);

            foreach (var method in methods)
            {
                Graph.Add(new HandlerCall(typeof(T), method));
            }

            _code = new Lazy<string>(() => Graph.GenerateCode());
        }

        public string theCode => _code.Value;
    }
}