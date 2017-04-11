using Jasper.Codegen.Compilation;

namespace Jasper.Codegen.StructureMap
{
    public class NestedContainerCreation : Frame
    {
        public NestedContainerCreation(Variable root) : base(false)
        {
            uses.Add(root);

            Wraps = true;
        }

        public override void GenerateCode(IGenerationModel generationModel, ISourceWriter writer)
        {
            writer.UsingBlock("var nested = _root.GetNestedContainer()", w => Next?.GenerateCode(generationModel, writer));
        }
    }
}