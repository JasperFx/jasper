using System.Collections.Generic;
using Jasper.Internals.Codegen;

namespace Jasper.Internals.IoC
{
    public class KnownVariableBuildStep : BuildStep
    {
        public new Variable Variable { get; }

        public KnownVariableBuildStep(Variable variable) : base(variable.VariableType, true, false)
        {
            Variable = variable;
        }

        public override IEnumerable<BuildStep> ReadDependencies(BuildStepPlanner planner)
        {
            yield break;
        }

        protected override Variable buildVariable()
        {
            return Variable;
        }
    }
}
