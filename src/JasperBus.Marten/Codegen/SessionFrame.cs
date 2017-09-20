using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Marten;

namespace JasperBus.Marten.Codegen
{
    public class SessionFrame : Frame
    {
        private Variable _store;

        public SessionFrame() : base(true)
        {
            Session = new Variable(typeof(IDocumentSession), this);
        }

        public Variable Session { get; }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            _store = chain.FindVariable(typeof(IDocumentStore));
            return new[] {_store};
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"BLOCK:using (var {Session.Usage} = {_store.Usage}.{nameof(IDocumentStore.OpenSession)}())");
            Next?.GenerateCode(method, writer);
            writer.FinishBlock();
        }
    }
}