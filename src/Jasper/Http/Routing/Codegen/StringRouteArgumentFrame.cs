using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;

namespace Jasper.Http.Routing.Codegen
{
    public class StringRouteArgumentFrame : RouteArgumentFrame
    {
        public string Name { get; }


        public StringRouteArgumentFrame(string name, int position) : base(name, position, typeof(string))
        {
            Name = name;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {Name} = {RoutingFrames.Segments}[{Position}];");
            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }




    }
}
