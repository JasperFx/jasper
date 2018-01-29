using BlueMilk.Compilation;

namespace BlueMilk.Codegen.Frames
{
    public class ReturnFrame : SyncFrame
    {
        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteReturnStatement(method);
        }
    }
}
