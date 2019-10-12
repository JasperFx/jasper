using Baseline;
using JasperHttp.ContentHandling;
using JasperHttp.Model;
using Microsoft.AspNetCore.Mvc;

namespace JasperHttp.MVCExtensions
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
