using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Messaging.Sagas
{
    public class PullSagaIdFromEnvelopeFrame : SyncFrame
    {
        private Variable _envelope;

        public PullSagaIdFromEnvelopeFrame(Type sagaIdType)
        {
            SagaId = new Variable(sagaIdType, SagaFramePolicy.SagaIdVariableName, this);
        }

        public Variable SagaId { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (SagaId.VariableType == typeof(string))
            {
                writer.Write($"var {SagaId.Usage} = {_envelope.Usage}.{nameof(Envelope.SagaId)};");
                writer.Write($"if (string.{nameof(string.IsNullOrEmpty)}({SagaFramePolicy.SagaIdVariableName})) throw new {nameof(IndeterminateSagaStateIdException)}({_envelope.Usage});");
            }
            else
            {
                var typeNameInCode = SagaId.VariableType == typeof(Guid) ? typeof(Guid).FullName : SagaId.VariableType.NameInCode();

                writer.Write($"if (!{typeNameInCode}.TryParse({_envelope.Usage}.{nameof(Envelope.SagaId)}, out {typeNameInCode} sagaId)) throw new {typeof(IndeterminateSagaStateIdException).FullNameInCode()}({_envelope.Usage});");
            }

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _envelope = chain.FindVariable(typeof(Envelope));
            yield return _envelope;
        }


    }
}
