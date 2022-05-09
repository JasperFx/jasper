using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas;

public class StoreOrDeleteSagaStateFrame : SyncFrame
{
    private readonly Variable _document;
    private readonly Type _sagaHandlerType;
    private Variable? _handler;
    private Variable? _persistor;

    public StoreOrDeleteSagaStateFrame(Variable document, Type sagaHandlerType)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _sagaHandlerType = sagaHandlerType;

        var prop = document.VariableType.GetProperty("Id");
        if (prop == null)
        {
            throw new ArgumentOutOfRangeException(nameof(document),
                $"Saga state document type {document.VariableType.FullNameInCode()} does not have an Id property that is required for usage with the in memory saga persistence");
        }
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _handler = chain.FindVariable(_sagaHandlerType);
        yield return _handler;

        yield return _document;

        _persistor = chain.FindVariable(typeof(InMemorySagaPersistor));

        yield return _persistor;
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write($"BLOCK:if ({_handler!.Usage}.{nameof(StatefulSagaOf<string>.IsCompleted)})");
        writer.Write(
            $"{_persistor!.Usage}.{nameof(InMemorySagaPersistor.Delete)}<{_document.VariableType.FullNameInCode()}>({_document.Usage}.Id);");
        writer.FinishBlock();
        writer.Write("BLOCK:else");
        writer.Write($"{_persistor.Usage}.{nameof(InMemorySagaPersistor.Store)}({_document.Usage});");
        writer.FinishBlock();

        Next?.GenerateCode(method, writer);
    }
}
