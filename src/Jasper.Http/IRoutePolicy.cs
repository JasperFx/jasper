using Jasper.Http.Model;
using LamarCodeGeneration;

namespace Jasper.Http
{
    // SAMPLE: IRoutePolicy
    /// <summary>
    ///     Use to apply your own conventions or policies to HTTP route handlers
    /// </summary>
    public interface IRoutePolicy
    {
        /// <summary>
        ///     Called during bootstrapping to alter how the route handlers are configured
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="rules"></param>
        void Apply(RouteGraph graph, GenerationRules rules);
    }

    // ENDSAMPLE
}
