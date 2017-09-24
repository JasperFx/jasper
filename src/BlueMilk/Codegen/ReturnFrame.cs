using BlueMilk.Compilation;

namespace BlueMilk.Codegen
{
    public class ReturnFrame : Frame
    {
        public ReturnFrame() : base(false)
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteReturnStatement(method);
        }
    }
}
