using System;
using System.Collections.Generic;
using Jasper.Internals.Codegen;

namespace Jasper.Internals.IoC
{
    public abstract class BuildStep
    {
        private readonly Lazy<Variable> _variable;
        public Type ServiceType { get; }

        public BuildStep(Type serviceType, bool canBeReused, bool shouldDispose)
        {
            ServiceType = serviceType;
            CanBeReused = canBeReused;
            ShouldDispose = shouldDispose;

            _variable = new Lazy<Variable>(buildVariable);
        }

        public bool ShouldDispose { get; }

        public bool CanBeReused { get; }

        /// <summary>
        /// If you are creating multiple instances of the same concrete type, use
        /// this as a suffix on the variable
        /// </summary>
        // TODO -- think this will need to be moved up to the generated method scope and be
        // on Variable itself
        public int Number { get; set; }

        public abstract IEnumerable<BuildStep> ReadDependencies(BuildStepPlanner planner);

        protected abstract Variable buildVariable();

        public Variable Variable => _variable.Value;
    }
}
