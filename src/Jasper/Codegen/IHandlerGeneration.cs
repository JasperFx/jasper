using System;

namespace Jasper.Codegen
{
    public interface IHandlerGeneration
    {
        string ClassName { get; }
        Type BaseType { get; }
        AsyncMode AsyncMode { get; }
        Variable InputVariable { get; }

        Frame Top { get; }

        InjectedField[] Fields { get; }
        Variable FindVariable(Type type);
    }
}