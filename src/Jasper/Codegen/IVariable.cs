using System;

namespace Jasper.Codegen
{
    public interface IVariable
    {
        string Name { get; }

        Type VariableType { get; }


    }
}