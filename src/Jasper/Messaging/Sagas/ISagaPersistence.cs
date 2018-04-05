using System;
using System.Linq;
using System.Reflection;
using Baseline.Reflection;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;

namespace Jasper.Messaging.Sagas
{
    public interface ISagaPersistence
    {
        Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState);
        Type DetermineSagaIdType(Type sagaStateType);
    }

    public class InMemorySagaPersistence : ISagaPersistence
    {
        public Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState)
        {
            loadedState = null;
            return new CommentFrame("Placeholder for real saga persistence");
        }

        public Type DetermineSagaIdType(Type sagaStateType)
        {
            var prop = FindIdProperty(sagaStateType);

            return prop.PropertyType;
        }

        private static PropertyInfo FindIdProperty(Type sagaStateType)
        {
            var prop = sagaStateType.GetProperties().FirstOrDefault(x => x.HasAttribute<SagaIdentityAttribute>())
                       ?? sagaStateType.GetProperties().FirstOrDefault(x => x.Name == "Id");
            return prop;
        }
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
