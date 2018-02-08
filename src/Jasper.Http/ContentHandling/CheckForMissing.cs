using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;

namespace Jasper.Http.ContentHandling
{
    public class CheckForMissing : IfBlock
    {
        public CheckForMissing(int statusCode, Variable item) : base($"{item.Usage} == null", new SetStatusCode(statusCode), new ReturnFrame())
        {
        }
    }
}
