using System.Collections.Generic;
using System.Linq;
using Baseline;
using JasperHttp.Routing.Codegen;
using LamarCodeGeneration;

namespace JasperHttp.Routing
{
    public class RouteNode
    {
        private readonly LightweightCache<string, RouteNode> _children;

        public string Segment { get; }

        public int Depth { get; }

        public int LeafDepth => Depth + 1;

        public RouteNode(string segment, int depth)
        {
            Segment = segment;
            Depth = depth;

            _children = new LightweightCache<string, RouteNode>(s => new RouteNode(s, Depth + 1));
        }

        public IList<Route> Leaves { get; } = new List<Route>();

        public Route SpreadRoute { get; set; }

        public IList<Route> ArgRoutes { get; } = new List<Route>();

        public IEnumerable<Route> ComplexArgRoutes => ArgRoutes.Where(x => x.Segments.Count() > LeafDepth);

        public bool TryFindLeafArgRoute(out Route route)
        {
            route = ArgRoutes.SingleOrDefault(x => x.Segments.Count() == LeafDepth);
            return route != null;
        }

        public RouteNode ChildFor(string segment)
        {
            return _children[segment];
        }

        public virtual void WriteSelectCode(ISourceWriter writer)
        {

            foreach (var route in ComplexArgRoutes)
            {
                writer.WriteComment("Look for odd shaped routes with complex parameter structures");
                writer.Write($"if (Matches{route.VariableName}(segments)) return {route.VariableName};");
            }


            if (_children.Any())
            {
                writer.Write($"BLOCK:if (segments.Length > {LeafDepth})");
                foreach (var node in _children)
                {
                    writer.IfCurrentSegmentEquals(Depth, node.Segment, node.WriteSelectCode);
                }

                if (SpreadRoute != null)
                {
                    writer.Return(SpreadRoute);
                }

                writer.ReturnNull();

                writer.FinishBlock();


            }

            foreach (var leaf in Leaves.OrderBy(x => x.LastSegment))
            {
                writer.IfCurrentSegmentEquals(Depth, leaf.LastSegment, w => w.Return(leaf));
            }

            if (TryFindLeafArgRoute(out var leafArg))
            {
                writer.Return(leafArg);
            }

            if (SpreadRoute != null)
            {
                writer.Return(SpreadRoute);
            }

            writer.ReturnNull();

        }
    }
}
