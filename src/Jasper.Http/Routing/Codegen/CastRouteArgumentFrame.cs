using System;
using LamarCodeGeneration;

namespace Jasper.Http.Routing.Codegen
{
    public class CastRouteArgumentFrame : RouteArgumentFrame
    {
        public CastRouteArgumentFrame(Type argType, string name, int position) : base(name, position, argType)
        {
            Name = name;
        }

        public string Name { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var {Name} = ({Variable.VariableType.FullNameInCode()}){Context.Usage}.Request.RouteValues[\"{Variable.Usage}\"];");
            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }
    }
}
