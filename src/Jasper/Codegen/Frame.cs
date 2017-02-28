using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public abstract class Frame
    {
        protected readonly IList<Frame> dependencies = new List<Frame>();
        internal readonly IList<Variable> creates = new List<Variable>();

        public bool IsAsync { get; }

        public Frame Next { get; set; }

        protected Frame(bool isAsync)
        {
            IsAsync = isAsync;
        }

        public virtual IEnumerable<Variable> Creates => creates;

        public abstract void GenerateCode(IHandlerGeneration generation, ISourceWriter writer);

        public virtual void ResolveVariables(IHandlerGeneration chain)
        {
            // Nothing
        }

        public virtual bool CanReturnTask() => false;

        public virtual void DetermineDependencies(IHandlerGeneration generation)
        {
            // Nothing
        }

        public Frame[] Dependencies => dependencies.ToArray();
    }
}