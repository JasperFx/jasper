using System;

namespace BlueMilk.Codegen.Variables
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);
    }
}
