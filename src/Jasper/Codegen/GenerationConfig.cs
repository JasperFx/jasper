using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;

namespace Jasper.Codegen
{
    public interface IGenerationConfig
    {
        string ApplicationNamespace { get; }
        IEnumerable<IVariableSource> Sources { get; }
        IEnumerable<Assembly> Assemblies { get; }
    }

    public class GenerationConfig : IGenerationConfig
    {
        public GenerationConfig(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
        }

        public string ApplicationNamespace { get; }

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        public readonly IList<Assembly> Assemblies = new List<Assembly>();

        IEnumerable<IVariableSource> IGenerationConfig.Sources => Sources;

        IEnumerable<Assembly> IGenerationConfig.Assemblies => Assemblies;
    }

    public class CompositeGenerationConfig : IGenerationConfig
    {
        private readonly GenerationConfig _core;
        private readonly GenerationConfig _feature;

        public CompositeGenerationConfig(GenerationConfig core, GenerationConfig feature)
        {
            _core = core;
            _feature = feature;
        }

        public string ApplicationNamespace => _core.ApplicationNamespace;
        public IEnumerable<IVariableSource> Sources => _feature.Sources.Concat(_core.Sources);
        public IEnumerable<Assembly> Assemblies => _feature.Assemblies.Concat(_core.Assemblies);
    }
}