using System.Collections.Generic;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;

namespace Jasper.Http.Routing.Codegen
{
    public class StringRouteArgument : Frame
    {
        public string Name { get; }
        public int Position { get; }

        public StringRouteArgument(string name, int position) : base((bool) false)
        {
            Name = name;
            Position = position;

            Variable = new Variable(typeof(string), Name, this);
        }

        public override IEnumerable<Variable> Creates
        {
            get { yield return Variable; }
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {Name} = {RoutingFrames.Segments}[{Position}];");
            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            Segments = chain.FindVariableByName(typeof(string[]), RoutingFrames.Segments);
            yield return Segments;
        }

        public Variable Variable { get; }
        public Variable Segments { get; private set; }
    }
}
