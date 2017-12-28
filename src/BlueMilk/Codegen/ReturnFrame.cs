using Jasper.Internals.Compilation;

namespace Jasper.Internals.Codegen
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
