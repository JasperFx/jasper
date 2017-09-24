using System;
using System.Collections.Generic;
using System.Linq;
using BlueMilk.Compilation;
using BlueMilk.Util;

namespace BlueMilk.Codegen
{
    public abstract class Frame
    {
        protected readonly IList<Frame> dependencies = new List<Frame>();
        protected internal readonly IList<Variable> creates = new List<Variable>();
        protected internal readonly IList<Variable> uses = new List<Variable>();
        private bool _hasResolved;

        public bool IsAsync { get; }
        public bool Wraps { get; protected set; } = false;

        public Frame Next { get; set; }

        protected Frame(bool isAsync)
        {
            IsAsync = isAsync;
        }



        public IEnumerable<Variable> Uses => uses;

        public virtual IEnumerable<Variable> Creates => creates;

        public abstract void GenerateCode(GeneratedMethod method, ISourceWriter writer);

        public void ResolveVariables(GeneratedMethod method)
        {
            // This has to be idempotent
            if (_hasResolved) return;

            var variables = FindVariables(method);
            if (variables.Any(x => x == null))
            {
                throw new InvalidOperationException($"Frame {this} could not resolve one of its variables");
            }

            uses.AddRange(variables);

            _hasResolved = true;
        }

        public virtual IEnumerable<Variable> FindVariables(GeneratedMethod chain)
        {
            yield break;
        }

        public virtual bool CanReturnTask() => false;


        public Frame[] Dependencies => dependencies.ToArray();

        public IEnumerable<Frame> AllFrames()
        {
            var frame = this;
            while (frame != null)
            {
                yield return frame;
                frame = frame.Next;
            }

        }
    }
}
