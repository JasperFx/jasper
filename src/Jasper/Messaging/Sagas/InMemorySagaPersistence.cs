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
        public Frame DeterminePersistenceFrame(SagaStateExistence existence, ref Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState)
        {
            loadedState = existingState;
            if (existence == SagaStateExistence.New)
            {
                var prop = FindIdProperty(sagaStateType);
                sagaId = new Variable(prop.PropertyType, existingState.Usage + "." + prop.Name);
            }

            var frame = new InMemorySagaPersistenceFrame(sagaStateType, sagaId, existence);

            if (existence == SagaStateExistence.Existing)
            {
                loadedState = frame.Document;
            }

            return frame;

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
