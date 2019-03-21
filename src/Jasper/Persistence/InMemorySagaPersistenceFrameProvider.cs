using System;
using Jasper.Configuration;
using Jasper.Messaging.Sagas;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Persistence
{
    public class InMemorySagaPersistenceFrameProvider : BaseSagaPersistenceFrameProvider
    {
        public override Frame DetermineStoreOrDeleteFrame(Variable document, Type sagaHandlerType)
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
