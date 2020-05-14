using System;
using System.Collections.Generic;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Http;

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
        public override IEnumerable<Variable> Creates
        {
            get { yield return Variable; }
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            Context = chain.FindVariable(typeof(HttpContext));
            yield return Context;
        }

        public Variable Context { get; private set; }
    }
}
