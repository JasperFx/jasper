using System.Collections.Generic;

namespace Jasper.Codegen
{
    public interface IDependentVariable : IVariable
    {
        IEnumerable<IVariable> Dependencies { get; }
    }
}