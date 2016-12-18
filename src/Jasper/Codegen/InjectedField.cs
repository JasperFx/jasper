using System;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public class InjectedField : Variable
    {
        public InjectedField(Type argType) : this(argType, DefaultArgName(argType))
        {
        }

        public InjectedField(Type argType, string name) : base(argType, "_" + name, VariableCreation.Injected)
        {
            CtorArg = name;
            ArgType = argType;
        }

        public Type ArgType { get; }

        public string CtorArg { get; }

        public string CtorArgDeclaration => $"{ArgType.FullName} {CtorArg}";

        public void WriteDeclaration(ISourceWriter writer)
        {
            writer.Write($"private readonly {ArgType.FullName} {Name};");
        }

        public void WriteAssignment(ISourceWriter writer)
        {
            writer.Write($"{Name} = {CtorArg};");
        }
    }
}