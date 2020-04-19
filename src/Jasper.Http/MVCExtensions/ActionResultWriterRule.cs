using Baseline;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.Http.MVCExtensions
{
    public class ActionResultWriterRule : IWriterRule
    {
        public bool TryToApply(RouteChain chain)
        {
            if (chain.ResourceType.CanBeCastTo<IActionResult>())
            {
                chain.Postprocessors.Add(new BuildActionContext());
                chain.Postprocessors.Add(new CallActionResultFrame(chain.ResourceType));

                return true;
            }

            return false;
        }
    }
}
