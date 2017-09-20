using System.Collections.Generic;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Marten;

namespace JasperBus.Marten.Codegen
{
    public class QuerySessionFrame : Frame
    {
        private Variable _store;

        public QuerySessionFrame() : base(false)
        {
            Session = new Variable(typeof(IQuerySession), this);
        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            _store = chain.FindVariable(typeof(IDocumentStore));
            return new[] {_store};
        }

        public Variable Session { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"BLOCK:using (var {Session.Usage} = {_store.Usage}.{nameof(IDocumentStore.QuerySession)}())");
            Next?.GenerateCode(method, writer);
            writer.FinishBlock();
        }
    }
}