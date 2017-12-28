using System;

namespace BlueMilk.Codegen
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);
    }
}
