using System;

namespace Jasper.Codegen
{
    public class CtorDependencySource : IVariableSource
    {


        public bool Matches(Type type)
        {
            return true;
        }

        public IVariable Create(Type type)
        {
            return new InjectedField(type);
        }
    }
}