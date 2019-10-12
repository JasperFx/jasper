using System.Collections.Generic;
using JasperHttp.Model;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.ContentHandling
{
    public class SetStatusCode : Frame
    {
        private readonly Variable _return;
        private readonly string _usage;
        private Variable _response;

        public SetStatusCode(int returnCode) : base(false)
        {
            _usage = returnCode.ToString();
        }

        public SetStatusCode(RouteChain chain) : base(false)
        {
            _return = chain.Action.ReturnVariable;
            _usage = _return.Usage;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"{_response.Usage}.{nameof(HttpResponse.StatusCode)} = {_usage};");

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            if (_return != null) yield return _return;

            _response = chain.FindVariable(typeof(HttpResponse));
            yield return _response;
        }
    }
}
