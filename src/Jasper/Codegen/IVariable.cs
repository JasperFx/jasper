using System;
using System.Collections.Generic;
using Baseline;

namespace Jasper.Codegen
{
    /*
First test,

1.) Input of some sort that has a single property
2.) Frame that sets the property on the input
3.) generate the class!

*/


    public interface IVariableSource
    {
        bool Matches(Type type);
        Variable Create(Type type);
    }

    public abstract class Variable
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

        public virtual IEnumerable<Variable> Dependencies
        {
            get
            {
                yield break;
            }
        }

        public virtual void PlaceAdditionalFrames(Frame frame)
        {
        }
    }
}