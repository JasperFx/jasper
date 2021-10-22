using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.EntityFrameworkCore;

namespace Jasper.Persistence.EntityFrameworkCore.Codegen
{
    public class TransactionalFrame : AsyncFrame
    {
        private readonly Type _dbContextType;
        private readonly IList<Loaded> _loadedDocs = new List<Loaded>();

        private readonly IList<Variable> _saved = new List<Variable>();
        private Variable _context;

        public TransactionalFrame(Type dbContextType)
        {
            _dbContextType = dbContextType;
        }

        public Variable Context { get; private set; }

        public Variable LoadDocument(Type documentType, Variable docId)
        {
            var document = new Variable(documentType, this);
            var loaded = new Loaded(document, documentType, docId);
            _loadedDocs.Add(loaded);

            return document;
        }

        public void InsertEntity(Variable document)
        {
            _saved.Add(document);
        }


        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            Context = chain.FindVariable(_dbContextType);

            // Inside of messaging. Not sure how this is gonna work for HTTP yet
            _context = chain.TryFindVariable(typeof(IExecutionContext), VariableSource.NotServices) ?? chain.FindVariable(typeof(IExecutionContext));

            if (_context != null) yield return _context;
            if (Context != null) yield return Context;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write(
                    $"await {typeof(JasperEnvelopeEntityFrameworkCoreExtensions).FullName}.{nameof(JasperEnvelopeEntityFrameworkCoreExtensions.EnlistInTransaction)}({_context.Usage}, {Context.Usage});");

            foreach (var loaded in _loadedDocs) loaded.Write(writer, Context);

            Next?.GenerateCode(method, writer);


            foreach (var saved in _saved)
                writer.Write($"{Context.Usage}.{nameof(DbContext.Add)}({saved.Usage});");

            writer.BlankLine();
            writer.WriteComment("Commit the unit of work");
            writer.Write($"await {Context.Usage}.{nameof(DbContext.SaveChangesAsync)}();");

        }

        public class Loaded
        {
            private readonly Variable _docId;
            private readonly Variable _document;
            private readonly Type _documentType;

            public Loaded(Variable document, Type documentType, Variable docId)
            {
                if (documentType == null) throw new ArgumentNullException(nameof(documentType));
                _documentType = documentType;

                _document = document ?? throw new ArgumentNullException(nameof(document));

                _docId = docId ?? throw new ArgumentNullException(nameof(docId));
            }

            public void Write(ISourceWriter writer, Variable session)
            {
                writer.Write(
                    $"var {_document.Usage} = await {session.Usage}.{nameof(DbContext.FindAsync)}<{_documentType.FullNameInCode()}>({_docId.Usage});");
            }
        }
    }
}
