using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Messaging.Sagas
{
    public class AssertSagaStateExistsFrame : SyncFrame
    {
        private readonly Variable _sagaState;
        private readonly Variable _sagaId;

        public AssertSagaStateExistsFrame(Variable sagaState, Variable sagaId)
        {
            _sagaState = sagaState;
            _sagaId = sagaId;
            uses.Add(_sagaState);
            uses.Add(_sagaId);
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"if ({_sagaState.Usage} == null) throw new {typeof(UnknownSagaStateException)}(typeof({_sagaState.VariableType.FullNameInCode()}), {_sagaId.Usage});");
            Next?.GenerateCode(method, writer);
        }


    }
}
