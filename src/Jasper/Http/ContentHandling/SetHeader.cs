using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.ContentHandling
{
    public class SetHeader : Frame
    {
        private Variable _response;
        public string Name { get; }
        public string Value { get; }

        public SetHeader(string name, string value) : base(false)
        {
            Name = name;
            Value = value;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"{_response.Usage}.Headers[\"{Name}\"] = \"{Value}\";");
            Next?.GenerateCode(method, writer);
        }

        protected internal override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            _response = chain.FindVariable(typeof(HttpResponse));

            yield return _response;
        }
    }
}