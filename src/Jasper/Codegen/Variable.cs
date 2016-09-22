using System;
using Baseline;

namespace Jasper.Codegen
{
    public class Variable : IVariable
    {
        public static string DefaultArgName(Type argType)
        {
            return argType.Name.SplitPascalCase().ToLower().Replace(" ", "_");
        }

        public Variable(Type argType) : this(argType, DefaultArgName(argType))
        {
        }

        public Variable(Type argType, string name)
        {
            Name = name;
            VariableType = argType;
        }

        public string Name { get; }
        public Type VariableType { get; }
    }
}