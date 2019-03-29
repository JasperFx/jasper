using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Marten;

namespace Jasper.Persistence.Marten.Codegen
{
    public class QuerySessionFrame : Frame
    {
        private Variable _store;

        public QuerySessionFrame() : base(false)
        {
            Session = new Variable(typeof(IQuerySession), this);
        }

        public Variable Session { get; }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _store = chain.FindVariable(typeof(IDocumentStore));
            return new[] {_store};
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"BLOCK:using (var {Session.Usage} = {_store.Usage}.{nameof(IDocumentStore.QuerySession)}())");
            Next?.GenerateCode(method, writer);
            writer.FinishBlock();
        }
    }
}
