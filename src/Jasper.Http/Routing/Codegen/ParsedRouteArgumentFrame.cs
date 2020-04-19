using System;
using System.Collections.Generic;
using Jasper.Http.Model;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
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
            var alias = RouteArgument.TypeOutputs[Variable.VariableType];
            writer.WriteLine($"{alias} {Variable.Usage};");
            writer.Write($"BLOCK:if (!{alias}.TryParse((string){Context.Usage}.Request.RouteValues[\"{Variable.Usage}\"], out {Variable.Usage}))");
            writer.WriteLine(
                $"{RouteGraph.Context}.{nameof(HttpContext.Response)}.{nameof(HttpResponse.StatusCode)} = 400;");
            writer.WriteLine(method.ToExitStatement());
            writer.FinishBlock();

            writer.BlankLine();
            Next?.GenerateCode(method, writer);
        }

    }
}
