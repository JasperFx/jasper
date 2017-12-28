using System;
using Baseline;
using Jasper.Internals.Util;

namespace Jasper.Internals.Codegen.ServiceLocation
{
    public class NoArgConcreteCreator : IVariableSource
    {
        public bool Matches(Type type)
        {
            return type.IsConcreteWithDefaultCtor();
        }

        public Variable Create(Type type)
        {
            return new NoArgCreationVariable(type);
        }
    }
}
