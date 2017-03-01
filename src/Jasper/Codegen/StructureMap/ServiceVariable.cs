using System;

namespace Jasper.Codegen.StructureMap
{
    public class ServiceVariable : Variable
    {
        public ServiceVariable(Type argType, NestedContainerVariable parent) : base(argType, DefaultArgName(argType))
        {
            Dependencies.Add(parent);
            Creator = new NestedContainerServiceCreation(this, parent);
        }
    }
}