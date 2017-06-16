using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using JasperHttp.Routing.Codegen;

namespace JasperHttp.Model
{
    public class SegmentsFrame : Frame
    {
        public SegmentsFrame() : base((bool) false)
        {
            Segments = new Variable(typeof(string[]), RoutingFrames.Segments, this);
            Creates = new[] {Segments};
        }

        public Variable Segments { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            // TODO -- move most of the code to a helper method
            writer.WriteLine($"var {RoutingFrames.Segments} = (string[]){RouteGraph.Context}.Items[\"{RoutingFrames.Segments}\"];");

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> Creates { get; } 
    }
}