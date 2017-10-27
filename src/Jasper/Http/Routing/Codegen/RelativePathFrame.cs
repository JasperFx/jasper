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
            throw new System.NotImplementedException();
        }
    }

    public class PathSegmentsFrame : RouteArgumentFrame
    {
        public PathSegmentsFrame(int position) : base(Route.PathSegments, position, typeof(string[]))
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
