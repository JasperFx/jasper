using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Mvc;

namespace Jasper.MvcExtender
{
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
}
