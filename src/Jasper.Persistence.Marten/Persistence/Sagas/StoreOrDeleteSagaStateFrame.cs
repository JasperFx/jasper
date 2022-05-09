using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Marten;

namespace Jasper.Persistence.Marten.Persistence.Sagas;

public class StoreOrDeleteSagaStateFrame : AsyncFrame
{
    private readonly Variable _document;
    private readonly Type _sagaHandlerType;
    private Variable? _handler;
    private Variable? _session;

    public StoreOrDeleteSagaStateFrame(Variable document, Type sagaHandlerType)
    {
        _document = document;
        _sagaHandlerType = sagaHandlerType;
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write($"BLOCK:if ({_handler!.Usage}.{nameof(StatefulSagaOf<string>.IsCompleted)})");
        writer.Write($"{_session!.Usage}.{nameof(IDocumentSession.Delete)}({_document.Usage});");
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
