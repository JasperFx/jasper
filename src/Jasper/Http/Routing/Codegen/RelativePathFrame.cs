using Jasper.Bus.Model;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;

namespace Jasper.Http.Routing.Codegen
{
    public class RelativePathFrame : RouteArgumentFrame
    {
        public RelativePathFrame(int position) : base(Route.RelativePath, position, typeof(string))
        {

        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = {nameof(MessageHandler.ToRelativePath)}({Segments.Usage}, {Position});");
        }
    }

    public class PathSegmentsFrame : RouteArgumentFrame
    {
        public PathSegmentsFrame(int position) : base(Route.PathSegments, position, typeof(string[]))
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Variable.Usage} = {nameof(MessageHandler.ToPathSegments)}({Segments.Usage}, {Position});");
        }
    }
}
