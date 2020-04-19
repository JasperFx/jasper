using LamarCodeGeneration.Frames;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.Http.MVCExtensions
{
    public class BuildActionContext : ConstructorFrame<ActionContext>
    {
        public BuildActionContext() : base(() => new ActionContext())
        {
            Set(x => x.HttpContext);
        }
    }
}
