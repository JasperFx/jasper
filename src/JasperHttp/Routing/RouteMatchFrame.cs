using System;
using System.Collections.Generic;
using System.Linq;
using JasperHttp.Routing.Codegen;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace JasperHttp.Routing
{
    public class RouteMatchFrame : SyncFrame
    {
        private readonly Route _route;
        private Variable _segments;

        public RouteMatchFrame(Route route)
        {
            _route = route;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteComment($"Trying to match the pattern '{_route.Pattern}'");

            writer.WriteComment("First check the route length");
            writer.Write($"if ({_segments.Usage}.{nameof(Array.Length)} != {_route.Segments.Count()}) return false;");

            var firstSegment = _route.Segments.OfType<RouteArgument>().First();
            var segments = _route.Segments.ToArray();

            for (int i = firstSegment.Position; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment is Segment path)
                {
                    writer.Write($"if (!{nameof(RouteSelector.Matches)}(segments[{i}], \"{path.SegmentPath}\")) return false;");
                }
            }



            writer.Write("return true;");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _segments = chain.FindVariable(typeof(string[]));
            yield return _segments;
        }
    }
}
