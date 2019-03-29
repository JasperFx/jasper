using System;
using System.Collections.Generic;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Http.Routing.Codegen
{
    public abstract class RouteArgumentFrame : SyncFrame
    {
        protected RouteArgumentFrame(string name, int position, Type variableType)
        {
            Variable = new Variable(variableType, name, this);
            Position = position;
        }

        public int Position { get; }
        public Variable Variable { get; }
        public Variable Segments { get; private set; }

        public override IEnumerable<Variable> Creates
        {
            get { yield return Variable; }
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            Segments = chain.FindVariableByName(typeof(string[]), RoutingFrames.Segments);
            yield return Segments;
        }
    }
}
