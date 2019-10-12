using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.ContentHandling
{
    public class SetHeader : Frame
    {
        private Variable _response;

        public SetHeader(string name, string value) : base(false)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"{_response.Usage}.Headers[\"{Name}\"] = \"{Value}\";");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _response = chain.FindVariable(typeof(HttpResponse));

            yield return _response;
        }
    }
}
