using System.Collections.Generic;
using System.Text;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;

namespace Jasper.Http.ContentHandling
{
    public class GetBytes : Frame
    {
        public Variable Text { get; }

        public GetBytes(Variable text) : base(false)
        {
            Text = text;

            Bytes = new Variable(typeof(byte[]), "bytes", this);
        }

        public Variable Bytes { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"var bytes = {typeof(Encoding).FullName}.{nameof(Encoding.UTF8)}.{nameof(Encoding.UTF8.GetBytes)}({Text.Usage});");
            Next?.GenerateCode(method, writer);
        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            yield return Text;
        }
    }
}