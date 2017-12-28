using System;

namespace Jasper.Internals.Codegen
{
    public class CastVariable : Variable
    {
        public CastVariable(Variable parent, Type specificType) : base(specificType, $"(({specificType.FullName}){parent.Usage})")
        {
            Dependencies.Add(parent);
        }
    }
}