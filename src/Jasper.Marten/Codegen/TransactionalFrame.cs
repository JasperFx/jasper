using System.Collections.Generic;
using Jasper.Messaging;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Marten;

namespace Jasper.Marten.Codegen
{
    public class TransactionalFrame : Frame
    {
        private Variable _store;
        private Variable _context;
        private bool _isUsingPersistence;

        public TransactionalFrame() : base(true)
        {
            Session = new Variable(typeof(IDocumentSession), this);
        }

        public Variable Session { get; }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _store = chain.FindVariable(typeof(IDocumentStore));

            _isUsingPersistence = chain.IsUsingMartenPersistence();

            // Inside of messaging. Not sure how this is gonna work for HTTP yet
            _context = chain.TryFindVariable(typeof(IMessageContext), VariableSource.NotServices);

            if (_context == null)
            {
                return new[] {_store};
            }
            else
            {
                return new[] {_store, _context};
            }
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"BLOCK:using (var {Session.Usage} = {_store.Usage}.{nameof(IDocumentStore.LightweightSession)}())");
            if (_context != null && _isUsingPersistence)
            {
                writer.Write($"await {typeof(MessagingExtensions).FullName}.{nameof(MessagingExtensions.EnlistInTransaction)}({_context.Usage}, {Session.Usage});");
            }

            Next?.GenerateCode(method, writer);
            writer.Write($"await {Session.Usage}.{nameof(IDocumentSession.SaveChangesAsync)}();");
            writer.FinishBlock();
        }
    }
}
