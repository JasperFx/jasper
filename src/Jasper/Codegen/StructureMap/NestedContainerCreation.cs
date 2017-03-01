using Jasper.Codegen.Compilation;

namespace Jasper.Codegen.StructureMap
{
    public class NestedContainerCreation : Frame
    {
        public NestedContainerCreation(Variable root) : base(false)
        {
            uses.Add(root);
        }

        public override void GenerateCode(IHandlerGeneration generation, ISourceWriter writer)
        {
            writer.UsingBlock("var nested = _root.GetNestedContainer()", w => Next?.GenerateCode(generation, writer));
        }
    }
}