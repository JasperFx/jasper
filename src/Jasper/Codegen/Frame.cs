using System.Linq;
using Jasper.Codegen.Compilation;
using Jasper.Configuration;

namespace Jasper.Codegen
{
    public abstract class Frame : Node<Frame, HandlerChain>
    {
        public bool IsAsync { get; }

        protected Frame(bool isAsync)
        {
            IsAsync = isAsync;
        }

        public void GenerateCode(HandlerGeneration generation, ISourceWriter writer)
        {
            if (Instantiates.Any())
            {
                Instantiates[0].GenerateCreationCode(generation, this, writer);
            }
            else
            {
                generateCode(generation, writer);
            }
        }

        internal abstract void generateCode(HandlerGeneration generation, ISourceWriter writer);

        // Going to say that other policies will deal w/ the existence of wrappers
        // Go find necessary variables, add any necessary wrappers
        public virtual void ResolveVariables(HandlerGeneration chain)
        {
            // nothing
        }

        // Needs to expose all variables used by this frame,
        // including dependents
        // Use a visitor to find that?

        public virtual bool CanReturnTask() => false;

        public Variable[] Instantiates { get; set; } = new Variable[0];
    }
}