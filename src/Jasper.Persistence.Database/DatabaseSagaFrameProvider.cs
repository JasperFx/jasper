using System;
using Jasper.Messaging.Sagas;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Persistence.Database
{
    public class DatabaseSagaFrameProvider : BaseSagaPersistenceFrameProvider
    {
        public override Frame DetermineStoreOrDeleteFrame(MethodCall sagaHandler, Variable document,
            Type sagaHandlerType)
        {
            throw new NotImplementedException();
        }

        protected override Frame buildPersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            ref Variable loadedState)
        {
            throw new NotImplementedException();
        }
    }
}
