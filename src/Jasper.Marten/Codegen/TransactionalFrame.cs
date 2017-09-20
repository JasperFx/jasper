using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Marten;

namespace Jasper.Marten.Codegen
{
    public class TransactionalFrame : Frame
    {
        private Variable _store;

        public TransactionalFrame() : base(true)
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
            writer.Write($"await {Session.Usage}.{nameof(IDocumentSession.SaveChangesAsync)}();");
            writer.FinishBlock();
        }
    }
}
