using System;

namespace Jasper.Codegen
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        IVariable Create(Type type);
    }
}