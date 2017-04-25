using System;

namespace Jasper.Codegen
{
    [Obsolete("Going to replace this with the more general GeneratedMethod/GeneratedClass model")]
    public interface IGenerationModel : IGeneratedMethod
    {
        string ClassName { get; }
        Type BaseType { get; }

        Variable InputVariable { get; }

        Frame Top { get; }

        InjectedField[] Fields { get; }

    }
}
