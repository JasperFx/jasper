using System.Collections.Generic;
using Jasper.Http.Model;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Http.Routing.Codegen
{
    public class SegmentsFrame : Frame
    {
        public SegmentsFrame() : base(false)
        {
            Segments = new Variable(typeof(string[]), RoutingFrames.Segments, this);
            Creates = new[] {Segments};
        }

        public Variable Segments { get; }

        public override IEnumerable<Variable> Creates { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine(
                $"var {RoutingFrames.Segments} = (string[]){RouteGraph.Context}.Items[\"{RoutingFrames.Segments}\"];");

            Next?.GenerateCode(method, writer);
        }
    }
}
