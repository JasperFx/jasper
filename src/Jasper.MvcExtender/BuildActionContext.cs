using LamarCodeGeneration.Frames;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.MvcExtender
{
    public class BuildActionContext : ConstructorFrame<ActionContext>
    {
        public BuildActionContext() : base(() => new ActionContext())
        {
            Set(x => x.HttpContext);
        }
    }
}
