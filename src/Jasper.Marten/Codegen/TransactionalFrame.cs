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
        private Variable _session;
        private bool _createsSession;

        public TransactionalFrame() : base(true)
        {

        }

        public Variable Session { get; private set; }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _store = chain.FindVariable(typeof(IDocumentStore));

            Session = chain.TryFindVariable(typeof(IDocumentSession), VariableSource.NotServices);
            if (Session == null)
            {
                _createsSession = true;
                Session = new Variable(typeof(IDocumentSession), this);
            }

            _isUsingPersistence = chain.IsUsingMartenPersistence();

            // Inside of messaging. Not sure how this is gonna work for HTTP yet
            _context = chain.TryFindVariable(typeof(IMessageContext), VariableSource.NotServices);

            yield return _store;
            if (_context != null) yield return _context;
            if (_session != null) yield return _session;

        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (_createsSession)
            {
                writer.Write($"BLOCK:using (var {Session.Usage} = {_store.Usage}.{nameof(IDocumentStore.LightweightSession)}())");
            }

            if (_context != null && _isUsingPersistence)
            {
                writer.Write($"await {typeof(MessagingExtensions).FullName}.{nameof(MessagingExtensions.EnlistInTransaction)}({_context.Usage}, {Session.Usage});");
            }

            Next?.GenerateCode(method, writer);
            writer.Write($"await {Session.Usage}.{nameof(IDocumentSession.SaveChangesAsync)}();");

            if (_createsSession)
            {
                writer.FinishBlock();
            }
        }
    }
}
