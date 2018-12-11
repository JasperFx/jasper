using Jasper.Configuration;
using Jasper.Http.ContentHandling;
using Lamar;
using LamarCompiler;

namespace Jasper.Http.Model
{
    public class RouteHandlerBuilder
    {
        private readonly IContainer _container;
        private readonly JasperGenerationRules _generation;
        private readonly ConnegRules _rules;

        public RouteHandlerBuilder(IContainer container, ConnegRules rules, JasperGenerationRules generation)
        {
            _container = container;
            _rules = rules;
            _generation = generation;
        }

        public RouteHandler Build(RouteChain chain)
        {
            var generatedAssembly = new GeneratedAssembly(_generation);
            chain.AssemblyType(generatedAssembly, _rules, _generation);

            _container.CompileWithInlineServices(generatedAssembly);

            var handler = chain.CreateHandler(_container);
            handler.Chain = chain;


            return handler;
        }
    }
}
