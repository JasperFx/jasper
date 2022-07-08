using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas;

public class DisregardIfStateDoesNotExistFrame : SyncFrame
{
    private readonly Variable _saga;

    public DisregardIfStateDoesNotExistFrame(Variable saga)
    {
        _saga = saga;
        uses.Add(_saga);
    }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write($"if ({_saga.Usage} == null) return;");
        Next?.GenerateCode(method, writer);
    }
}
