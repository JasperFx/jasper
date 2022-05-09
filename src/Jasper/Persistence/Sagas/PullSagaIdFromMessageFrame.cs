﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas;

public class PullSagaIdFromMessageFrame : SyncFrame
{
    private readonly Type _messageType;
    private readonly PropertyInfo _sagaIdProperty;
    private Variable? _envelope;
    private Variable? _message;

    public PullSagaIdFromMessageFrame(Type messageType, PropertyInfo sagaIdProperty)
    {
        _messageType = messageType;
        _sagaIdProperty = sagaIdProperty;

        if (!SagaFramePolicy.ValidSagaIdTypes.Contains(_sagaIdProperty.PropertyType))
        {
            throw new ArgumentOutOfRangeException(nameof(messageType),
                $"SagaId must be one of {SagaFramePolicy.ValidSagaIdTypes.Select(x => x.NameInCode()).Join(", ")}");
        }

        SagaId = new Variable(sagaIdProperty.PropertyType, SagaFramePolicy.SagaIdVariableName, this);
    }

    public Variable SagaId { get; }

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        if (_sagaIdProperty.PropertyType == typeof(string))
        {
            writer.Write(
                $"{_sagaIdProperty.PropertyType.NameInCode()} {SagaFramePolicy.SagaIdVariableName} = {_envelope!.Usage}.{nameof(Envelope.SagaId)} ?? {_message!.Usage}.{_sagaIdProperty.Name};");
            writer.Write(
                $"if (string.{nameof(string.IsNullOrEmpty)}({SagaFramePolicy.SagaIdVariableName})) throw new {typeof(IndeterminateSagaStateIdException).FullName}({_envelope.Usage});");
        }
        else
        {
            var typeNameInCode = _sagaIdProperty.PropertyType == typeof(Guid)
                ? typeof(Guid).FullName
                : _sagaIdProperty.PropertyType.NameInCode();


            writer.Write(
                $"if (!{typeNameInCode}.TryParse({_envelope!.Usage}.{nameof(Envelope.SagaId)}, out {typeNameInCode} sagaId)) sagaId = {_message!.Usage}.{_sagaIdProperty.Name};");

            if (_sagaIdProperty.PropertyType == typeof(Guid))
            {
                writer.Write(
                    $"if ({SagaId.Usage} == System.Guid.Empty) throw new {typeof(IndeterminateSagaStateIdException).FullName}({_envelope.Usage});");
            }
            else
            {
                writer.Write(
                    $"if ({SagaId.Usage} == 0) throw new {typeof(IndeterminateSagaStateIdException).FullName}({_envelope.Usage});");
            }
        }


        Next?.GenerateCode(method, writer);
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _message = chain.FindVariable(_messageType);
        yield return _message;

        _envelope = chain.FindVariable(typeof(Envelope));
        yield return _envelope;
    }
}
