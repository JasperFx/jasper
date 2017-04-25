using System;

namespace Jasper.Codegen
{
    public interface IGeneratedMethod
    {
        Variable FindVariable(Type type);
        AsyncMode AsyncMode { get; }
    }
}