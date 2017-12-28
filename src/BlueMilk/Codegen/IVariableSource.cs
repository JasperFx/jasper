using System;

namespace Jasper.Internals.Codegen
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);
    }
}
