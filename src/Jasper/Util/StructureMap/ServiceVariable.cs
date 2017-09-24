using System;
using BlueMilk.Codegen;

namespace Jasper.Util.StructureMap
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