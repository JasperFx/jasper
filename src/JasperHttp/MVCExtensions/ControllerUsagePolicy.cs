using System.Linq;
using Baseline;
using Jasper.Configuration;
using JasperHttp.Model;
using Microsoft.AspNetCore.Mvc;

namespace JasperHttp.MVCExtensions
{
    // SAMPLE: ControllerUsagePolicy
    internal class ControllerUsagePolicy : IRoutePolicy
    {
        public void Apply(RouteGraph graph, JasperGenerationRules rules)
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
