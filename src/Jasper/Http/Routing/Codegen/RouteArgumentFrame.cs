using System;
using System.Collections.Generic;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;

namespace Jasper.Http.Routing.Codegen
{
    public abstract class RouteArgumentFrame : SyncFrame
    {
        public int Position { get; }
        public Variable Variable { get; }
        public Variable Segments { get; private set; }

        protected RouteArgumentFrame(string name, int position, Type variableType)
        {
            Variable = new Variable(variableType, name, this);
            Position = position;

        }

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
