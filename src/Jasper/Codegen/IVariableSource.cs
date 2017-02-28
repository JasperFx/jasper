using System;
using Jasper.Codegen.New;

namespace Jasper.Codegen
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);
    }
}