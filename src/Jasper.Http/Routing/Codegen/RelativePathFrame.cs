using System;
using System.Collections.Generic;
using Jasper.Http.Model;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Routing.Codegen
{
    public class RelativePathFrame : RouteArgumentFrame
    {
        private Variable _request;

        public RelativePathFrame(int position) : base(JasperRoute.RelativePath, position, typeof(string))
        {
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write(
                $"var {Variable.Usage} = {nameof(RouteHandler.ToRelativePath)}({_request.Usage});");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _request = chain.FindVariable(typeof(HttpRequest));
            yield return _request;
        }
    }
}
