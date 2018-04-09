using System;
using System.Linq;
using System.Reflection;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using ReflectionExtensions = Baseline.Reflection.ReflectionExtensions;

namespace Jasper.Messaging.Sagas
{
    public class InMemorySagaPersistence : ISagaPersistence
    {
        public Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState)
        {
            if (existence == SagaStateExistence.Existing)
            {
                var frame = new LoadDocumentFrame(sagaStateType, sagaId);
                loadedState = frame.Document;
                return frame;
            }

            loadedState = existingState;
            return null;

        }

        public Type DetermineSagaIdType(Type sagaStateType)
        {
            var prop = FindIdProperty(sagaStateType);

            return prop.PropertyType;
        }

        public Frame DetermineStoreOrDeleteFrame(Variable document, Type sagaHandlerType)
        {
            return new StoreOrDeleteSagaStateFrame(document, sagaHandlerType);
        }

        private static PropertyInfo FindIdProperty(Type sagaStateType)
        {
            var prop = sagaStateType.GetProperties()
                           .FirstOrDefault(ReflectionExtensions.HasAttribute<SagaIdentityAttribute>)
                       ?? sagaStateType.GetProperties().FirstOrDefault(x => x.Name == "Id");
            return prop;
        }
    }
}
