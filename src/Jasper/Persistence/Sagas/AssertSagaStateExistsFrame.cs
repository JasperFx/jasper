﻿using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas;

public class AssertSagaStateExistsFrame : SyncFrame
{
    private readonly Variable _sagaId;
    private readonly Variable _sagaState;

    public AssertSagaStateExistsFrame(Variable sagaState, Variable sagaId)
    {
        _sagaState = sagaState;
        _sagaId = sagaId;
        uses.Add(_sagaState);
        uses.Add(_sagaId);
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write(
            $"if ({_sagaState.Usage} == null) throw new {typeof(UnknownSagaStateException)}(typeof({_sagaState.VariableType.FullNameInCode()}), {_sagaId.Usage});");
        Next?.GenerateCode(method, writer);
    }
}
