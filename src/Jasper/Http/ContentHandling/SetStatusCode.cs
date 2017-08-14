using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.ContentHandling
{
    public class SetStatusCode : Frame
    {
        private Variable _response;
        private readonly Variable _return;

        public SetStatusCode(RouteChain chain) : base(false)
        {
            _return = chain.Action.ReturnVariable;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"{_response.Usage}.{nameof(HttpResponse.StatusCode)} = {_return.Usage};");

            Next?.GenerateCode(method, writer);
        }

        protected internal override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            yield return _return;

            _response = chain.FindVariable(typeof(HttpResponse));
            yield return _response;
        }
    }
}