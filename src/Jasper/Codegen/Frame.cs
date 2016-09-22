using System.Collections.Generic;
using Jasper.Codegen.Compilation;
using Jasper.Configuration;

namespace Jasper.Codegen
{
    public abstract class Frame : Node<Frame, HandlerChain>
    {
        public abstract void GenerateCode(HandlerChain chain, ISourceWriter writer);

        public virtual void Preprocess(HandlerChain chain)
        {
            // nothing
        }

        public virtual IEnumerable<IVariable> Variables
        {
            get
            {
                yield break;
            }
        }
    }
}