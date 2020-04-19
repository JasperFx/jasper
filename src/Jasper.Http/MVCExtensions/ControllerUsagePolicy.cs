using System.Linq;
using Baseline;
using Jasper.Http.Model;
using LamarCodeGeneration;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.Http.MVCExtensions
{
    // SAMPLE: ControllerUsagePolicy
    internal class ControllerUsagePolicy : IRoutePolicy
    {
        public void Apply(RouteGraph graph, GenerationRules rules)
        {
            graph
                .Where(x => x.Action.HandlerType.CanBeCastTo<ControllerBase>())
                .Each(x =>
                {
                    x.Middleware.Add(new BuildOutControllerContextFrame());
                    x.Middleware.Add(new SetControllerContextFrame(x.Action.HandlerType));
                });
        }
    }

    // ENDSAMPLE
}
