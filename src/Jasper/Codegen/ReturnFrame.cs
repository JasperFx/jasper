using Jasper.Codegen.Compilation;

namespace Jasper.Codegen
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
