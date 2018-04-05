using System;
using System.Collections.Generic;
using Jasper.Marten.Codegen;
using Jasper.Messaging.Sagas;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Marten;
using Marten.Schema;
using Marten.Util;

namespace Jasper.Marten.Persistence
{
    public class MartenSagaPersistence : ISagaPersistence
    {

        public Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState)
        {
            var frame = new TransactionalFrame();
            if (existence == SagaStateExistence.Existing)
            {
                var doc = frame.LoadDocument(sagaStateType, sagaId);
                loadedState = doc;
            }
            else
            {
                loadedState = existingState;
            }

            return frame;
        }

        public Type DetermineSagaIdType(Type sagaStateType)
        {
            var mapping = new DocumentMapping(sagaStateType, new StoreOptions());
            return mapping.IdMember.GetMemberType();
        }

        public Frame DetermineStoreOrDeleteFrame(Variable document, Type sagaHandlerType)
        {
            return new StoreOrDeleteSagaStateFrame(document, sagaHandlerType);
        }
    }

    public class StoreOrDeleteSagaStateFrame : SyncFrame
    {
        private readonly Variable _document;
        private readonly Type _sagaHandlerType;
        private Variable _handler;
        private Variable _session;

        public StoreOrDeleteSagaStateFrame(Variable document, Type sagaHandlerType)
        {
            _document = document;
            _sagaHandlerType = sagaHandlerType;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"BLOCK:if ({_handler.Usage}.{nameof(StatefulSagaOf<string>.IsCompleted)})");
            writer.Write($"{_session.Usage}.{nameof(IDocumentSession.Delete)}({_document.Usage});");
            writer.FinishBlock();
            writer.Write("BLOCK:else");
            writer.Write($"{_session.Usage}.{nameof(IDocumentSession.Store)}({_document.Usage});");
            writer.FinishBlock();

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _handler = chain.FindVariable(_sagaHandlerType);
            yield return _handler;

            yield return _document;

            _session = chain.FindVariable(typeof(IDocumentSession));
            yield return _session;
        }
    }
}
