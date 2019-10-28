using System;
using System.Linq;
using System.Reflection;
using Baseline.Reflection;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Lamar;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence
{
    public abstract class BaseSagaPersistenceFrameProvider : ISagaPersistenceFrameProvider
    {
        public abstract Frame DetermineStoreOrDeleteFrame(IContainer container, HandlerChain chain,
            MethodCall sagaHandler,
            Variable document,
            Type sagaHandlerType);

        public Type DetermineSagaIdType(Type sagaStateType)
        {
            var prop = findIdProperty(sagaStateType);

            return prop.PropertyType;
        }

        protected static PropertyInfo findIdProperty(Type sagaStateType)
        {
            var prop = sagaStateType.GetProperties()
                           .FirstOrDefault(x => ((MemberInfo) x).HasAttribute<SagaIdentityAttribute>())
                       ?? sagaStateType.GetProperties().FirstOrDefault(x => x.Name == "Id");
            return prop;
        }

        public Frame DeterminePersistenceFrame(IContainer container, HandlerChain chain, MethodCall sagaHandler,
            SagaStateExistence existence,
            ref Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState)
        {
            loadedState = existingState;
            if (existence == SagaStateExistence.New)
            {
                var prop = findIdProperty(sagaStateType);
                sagaId = new Variable(prop.PropertyType, existingState.Usage + "." + prop.Name);
            }

            var frame = buildPersistenceFrame(container, chain, existence, ref sagaId, sagaStateType, existingState, ref loadedState);

            return frame;
        }

        protected abstract Frame buildPersistenceFrame(IContainer container, HandlerChain chain,
            SagaStateExistence existence, ref Variable sagaId,
            Type sagaStateType, Variable existingState, ref Variable loadedState);
    }
}
