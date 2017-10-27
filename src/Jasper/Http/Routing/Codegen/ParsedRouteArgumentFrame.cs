using System;
using System.Collections.Generic;
using Jasper.Http.Model;
using Jasper.Internals.Codegen;
using Jasper.Internals.Compilation;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Routing.Codegen
{
    public class ParsedRouteArgumentFrame : RouteArgumentFrame
    {
        public ParsedRouteArgumentFrame(Type type, string name, int position) : base(name, position, type)
        {
        }

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
