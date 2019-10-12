using LamarCodeGeneration;

namespace JasperHttp.Routing.Codegen
{
    public class StringRouteArgumentFrame : RouteArgumentFrame
    {
        public StringRouteArgumentFrame(string name, int position) : base(name, position, typeof(string))
        {
            Name = name;
        }

        public string Name { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {Name} = {RoutingFrames.Segments}[{Position}];");
            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }


    }
}
