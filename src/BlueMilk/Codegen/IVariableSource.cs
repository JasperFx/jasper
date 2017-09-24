using System;

namespace BlueMilk.Codegen
{
    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);
    }

    public class StaticVariable : Variable, IVariableSource
    {
        public StaticVariable(Type variableType, string usage) : base(variableType, usage)
        {
        }

        public bool Matches(Type type)
        {
            return type == VariableType;
        }

        public Variable Create(Type type)
        {
            return this;
        }
    }
}