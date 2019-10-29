using System;
using System.Collections.Generic;
using Jasper.Messaging.Sagas;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.EntityFrameworkCore;

namespace Jasper.Persistence.EntityFrameworkCore.Codegen
{
    public class StoreOrDeleteSagaStateFrame : SyncFrame
    {
        private readonly Type _dbContextType;
        private readonly Variable _state;
        private readonly Type _sagaHandlerType;
        private Variable _handler;
        private Variable _context;

        public StoreOrDeleteSagaStateFrame(Type dbContextType, Variable state, Type sagaHandlerType)
        {
            _dbContextType = dbContextType;
            _state = state;
            _sagaHandlerType = sagaHandlerType;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteComment("Check if the saga has been completed");
            writer.Write($"BLOCK:if ({_handler.Usage}.{nameof(StatefulSagaOf<string>.IsCompleted)})");
            writer.WriteComment("Delete the saga state entity");
            writer.Write($"{_context.Usage}.{nameof(DbContext.Remove)}({_state.Usage});");
            writer.FinishBlock();
            writer.Write("BLOCK:else");
            writer.WriteComment("Persist the saga state entity");
            writer.Write($"{_context.Usage}.{nameof(DbContext.Add)}({_state.Usage});");
            writer.FinishBlock();

            writer.BlankLine();

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _handler = chain.FindVariable(_sagaHandlerType);
            yield return _handler;

            yield return _state;

            _context = chain.FindVariable(_dbContextType);
            yield return _context;
        }
    }
}
