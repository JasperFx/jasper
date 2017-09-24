using BlueMilk.Codegen;
using StructureMap;

namespace Jasper.Util.StructureMap
{
    public class NestedContainerVariable : Variable
    {
        public NestedContainerVariable(NestedContainerCreation creation) : base(typeof(IContainer), "nested", creation)
        {
        }
    }
}