using System;
using Jasper.Messaging.Sagas;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence
{
    public class InMemorySagaPersistenceFrameProvider : BaseSagaPersistenceFrameProvider
    {
        public override Frame DetermineStoreOrDeleteFrame(MethodCall sagaHandler, Variable document,
            Type sagaHandlerType)
        {
            return new StoreOrDeleteSagaStateFrame(document, sagaHandlerType);
        }

        protected override Frame buildPersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            ref Variable loadedState)
        {
            var frame = new InMemorySagaPersistenceFrame(sagaStateType, sagaId, existence);

            if (existence == SagaStateExistence.Existing) loadedState = frame.Document;

            return frame;
        }
    }
}
