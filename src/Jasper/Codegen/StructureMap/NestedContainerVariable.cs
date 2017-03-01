using StructureMap;

namespace Jasper.Codegen.StructureMap
{
    public class NestedContainerVariable : Variable
    {
        public NestedContainerVariable(NestedContainerCreation creation) : base(typeof(IContainer), "nested", creation)
        {
        }
    }
}