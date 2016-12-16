using Jasper.Codegen.Compilation;
using Jasper.Configuration;

namespace Jasper.Codegen
{
    /*
Types of Frame:
* Plain call
* Wrapping operation



*/

    public abstract class Frame : Node<Frame, HandlerChain>
    {
        public bool IsAsync { get; }

        protected Frame(bool isAsync)
        {
            IsAsync = isAsync;
        }

        public abstract void GenerateCode(HandlerGeneration generation, ISourceWriter writer);

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
    }
}