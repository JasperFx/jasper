using System;
using System.Collections.Generic;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas
{
    public class SetStateFrame : SyncFrame
    {
        private readonly Type _handlerType;
        private readonly Type _stateType;
        private Variable _handler;
        private Variable _state;

        public SetStateFrame(Type handlerType, Type stateType)
        {
            _handlerType = handlerType;
            _stateType = stateType;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"{_handler.Usage}.{nameof(StatefulSagaOf<string>.State)} = {_state.Usage};");

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _handler = chain.FindVariable(_handlerType);
            yield return _handler;

            _state = chain.FindVariable(_stateType);
            yield return _state;


        }
    }
}
