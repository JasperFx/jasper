using System;
using System.Collections.Generic;
using Jasper.Internals.Compilation;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.Codegen.ServiceLocation
{
    public class ServiceScopeFactoryCreation : SyncFrame
    {
        private Variable _factory;
        private readonly Variable _scope;

        public ServiceScopeFactoryCreation()
        {
            _scope = new Variable(typeof(IServiceScope), this);
            Provider = new Variable(typeof(IServiceProvider), this);
        }

        public Variable Provider { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.UsingBlock($"var {_scope.Usage} = {_factory.Usage}.{nameof(IServiceScopeFactory.CreateScope)}()", w =>
            {
                w.Write($"var {Provider.Usage} = {_scope.Usage}.{nameof(IServiceScope.ServiceProvider)};");
                Next?.GenerateCode(method, w);
            });
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _factory = chain.FindVariable(typeof(IServiceScopeFactory));
            yield return _factory;
        }
    }
}
