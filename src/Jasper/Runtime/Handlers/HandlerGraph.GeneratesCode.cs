using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.ImTools;
using BaselineTypeDiscovery;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;

namespace Jasper.Runtime.Handlers
{
    public partial class HandlerGraph
    {
        IReadOnlyList<ICodeFile> ICodeFileCollection.BuildFiles()
        {
            return Chains.ToList();
        }

        string ICodeFileCollection.ChildNamespace { get; } = "JasperHandlers";

        public GenerationRules Rules => _generation;
    }
}
