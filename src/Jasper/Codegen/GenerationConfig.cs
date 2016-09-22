using System.Collections.Generic;

namespace Jasper.Codegen
{
    public class GenerationConfig
    {
        public GenerationConfig(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
        }

        public string ApplicationNamespace { get; }

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();
    }
}