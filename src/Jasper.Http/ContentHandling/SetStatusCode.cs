using System.Collections.Generic;
using BlueMilk.Codegen;
using BlueMilk.Codegen.Frames;
using BlueMilk.Codegen.Variables;
using BlueMilk.Compilation;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.ContentHandling
{
    public class SetStatusCode : Frame
    {
        private Variable _response;
        private readonly Variable _return;
        private readonly string _usage;

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
