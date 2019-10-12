using JasperHttp.Model;
using LamarCodeGeneration;

namespace JasperHttp.Routing.Codegen
{
    public class PathSegmentsFrame : RouteArgumentFrame
    {
        public PathSegmentsFrame(int position) : base(Route.PathSegments, position, typeof(string[]))
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write(
                $"var {Variable.Usage} = {nameof(RouteHandler.ToPathSegments)}({Segments.Usage}, {Position});");
            Next?.GenerateCode(method, writer);
        }
    }
}
