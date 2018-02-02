using BlueMilk;
using BlueMilk.Codegen;
using BlueMilk.Compilation;
using Jasper.Http.ContentHandling;

namespace Jasper.Http.Model
{
    public class RouteHandlerBuilder
    {
        private readonly IContainer _container;
        private readonly ConnegRules _rules;
        private readonly GenerationRules _generation;

        public RouteHandlerBuilder(IContainer container, ConnegRules rules, GenerationRules generation)
        {
            _container = container;
            _rules = rules;
            _generation = generation;
        }

        public RouteHandler Build(RouteChain chain)
        {
            _rules.Apply(chain);

            var generatedAssembly = new GeneratedAssembly(_generation);
            chain.AssemblyType(generatedAssembly);

            _container.CompileWithInlineServices(generatedAssembly);

            var handler = chain.CreateHandler(_container);
            handler.Chain = chain;


            return handler;
        }
    }
}
