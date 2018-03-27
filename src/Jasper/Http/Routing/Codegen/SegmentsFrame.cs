using System.Collections.Generic;
using Jasper.Http.Model;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Http.Routing.Codegen
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
            writer.WriteLine($"var {RoutingFrames.Segments} = (string[]){RouteGraph.Context}.Items[\"{RoutingFrames.Segments}\"];");

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> Creates { get; }
    }
}
