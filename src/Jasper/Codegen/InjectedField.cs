using System;
using System.Collections.Generic;
using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
{
    public class InjectedField : Variable
    {
        public InjectedField(Type argType) : this(argType, DefaultArgName(argType))
        {
        }

        public InjectedField(Type argType, string name) : base(argType, "_" + name)
        {
            CtorArg = name;
            ArgType = argType;
        }

        public Type ArgType { get; }

        public string CtorArg { get; }

        public IEnumerable<IVariable> Dependencies => new IVariable[0];
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