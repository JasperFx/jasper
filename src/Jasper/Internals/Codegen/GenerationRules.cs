using System.Collections.Generic;
using System.Reflection;
using Jasper.Internals.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.Codegen
{
    public class GenerationRules
    {
        public GenerationRules(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
        }

        public string ApplicationNamespace { get; }

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        public readonly IList<Assembly> Assemblies = new List<Assembly>();

        public void ReadServices(IServiceCollection services)
        {
            Services = new ServiceGraph(services);
        }

        public ServiceGraph Services { get; private set; } = new ServiceGraph(new ServiceRegistry());
    }


}
