using System;
using System.Linq;
using System.Reflection;
using Baseline;

namespace Jasper.Http.Routing
{
    internal class DefaultRoutingRule : IPatternRule
    {
        public RoutePattern DetermineRoute(Type handlerType, MethodInfo method)
        {
            if (!HttpVerbs.All.Any(x => method.Name.StartsWith(x + "_", StringComparison.OrdinalIgnoreCase)))
                return null;

            var pattern = method.Name;
            pattern = pattern.Replace("___", "-").Replace("__", "_@");

            var parts = pattern.Split('_')
                .Select(x => x.Replace("@", "_")).ToArray();


            var verb = HttpVerbs.All.FirstOrDefault(x => x.EqualsIgnoreCase(parts[0]));
            if (verb.IsNotEmpty())
                parts = parts.Skip(1).ToArray();
            else
                verb = HttpVerbs.GET;

            var segments = parts
                .Select((x, position) => new Segment(x.ToLowerInvariant(), position))
                .OfType<ISegment>()
                .ToArray();


            return new RoutePattern(verb, parts.Join("/"), segments);

        }
    }
}
