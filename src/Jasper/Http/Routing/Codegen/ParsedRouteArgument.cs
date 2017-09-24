using System;
using System.Collections.Generic;
using BlueMilk.Codegen;
using BlueMilk.Compilation;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Routing.Codegen
{
    public class ParsedRouteArgument : Frame
    {
        public int Position { get; }



        public ParsedRouteArgument(Type type, string name, int position) : base((bool) true)
        {
            Position = position;
            Variable = new Variable(type, name);
        }

        public Variable Variable { get; }

        public override IEnumerable<Variable> Creates
        {
            get { yield return Variable; }
        }

        public override IEnumerable<Variable> FindVariables(GeneratedMethod chain)
        {
            Segments = chain.FindVariableByName(typeof(string[]), RoutingFrames.Segments);
            yield return Segments;
        }

        public Variable Segments { get; set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var alias = RoutingFrames.TypeOutputs[Variable.VariableType];
            writer.WriteLine($"{alias} {Variable.Usage};");
            writer.Write($"BLOCK:if (!{alias}.TryParse(segments[{Position}], out {Variable.Usage}))");
            writer.WriteLine($"{RouteGraph.Context}.{nameof(HttpContext.Response)}.{nameof(HttpResponse.StatusCode)} = 400;");
            writer.WriteLine(method.ToExitStatement());
            writer.FinishBlock();

            writer.BlankLine();
            Next?.GenerateCode(method, writer);
        }


    }



}
