using System;
using System.Collections.Generic;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Messaging.Sagas
{
    public class LoadDocumentFrame : SyncFrame
    {
        private readonly Variable _sagaId;
        private Variable _persistor;

        public LoadDocumentFrame(Type documentType, Variable sagaId)
        {
            _sagaId = sagaId;
            Document = new Variable(documentType, this);
        }

        public Variable Document { get; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"var {Document.Usage} = {_persistor.Usage}.{nameof(InMemorySagaPersistor.Load)}<{Document.VariableType.FullNameInCode()}>({_sagaId.Usage});");
            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield return _sagaId;

            _persistor = chain.FindVariable(typeof(InMemorySagaPersistor));

            yield return _persistor;
        }
    }
}
