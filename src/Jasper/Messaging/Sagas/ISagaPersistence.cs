using System;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Messaging.Sagas
{
    public interface ISagaPersistence
    {
        Frame DeterminePersistenceFrame(SagaStateExistence existence, ref Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState);

        Type DetermineSagaIdType(Type sagaStateType);

        Frame DetermineStoreOrDeleteFrame(Variable document, Type sagaHandlerType);
    }

    public class CommentFrame : SyncFrame
    {
        private readonly string _text;

        public CommentFrame(string text)
        {
            _text = text;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write("// " + _text);
        }
    }
}
