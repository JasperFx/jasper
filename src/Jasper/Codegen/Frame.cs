using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public abstract class Frame
    {
        protected readonly IList<Frame> dependencies = new List<Frame>();
        internal readonly IList<Variable> creates = new List<Variable>();
        internal readonly IList<Variable> uses = new List<Variable>();

        public bool IsAsync { get; }

        public Frame Next { get; set; }

        protected Frame(bool isAsync)
        {
            IsAsync = isAsync;
        }

        public IEnumerable<Variable> Uses => uses;

        public virtual IEnumerable<Variable> Creates => creates;

        public abstract void GenerateCode(IHandlerGeneration generation, ISourceWriter writer);

        public void ResolveVariables(IHandlerGeneration chain)
        {
            var variables = resolveVariables(chain);
            uses.AddRange(variables);
        }

        protected virtual IEnumerable<Variable> resolveVariables(IHandlerGeneration chain)
        {
            yield break;
        }

        public virtual bool CanReturnTask() => false;


        public Frame[] Dependencies => dependencies.ToArray();
    }
}