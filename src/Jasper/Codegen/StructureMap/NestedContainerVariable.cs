using Jasper.Codegen.Compilation;
using StructureMap;

namespace Jasper.Codegen.StructureMap
{
    public class NestedContainerCreation : Frame
    {
        public NestedContainerCreation() : base(false)
        {

        }

        public override void GenerateCode(IHandlerGeneration generation, ISourceWriter writer)
        {
            writer.UsingBlock("var nested = _root.GetNestedContainer()", w => Next?.GenerateCode(generation, writer));
        }
    }

    public class NestedContainerVariable : Variable
    {
        public NestedContainerVariable() : base(typeof(IContainer), "nested", new NestedContainerCreation())
        {
            Dependencies.Add(StructureMapServices.Root);
        }
    }
}