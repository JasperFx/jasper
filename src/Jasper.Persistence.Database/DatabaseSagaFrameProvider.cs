using System;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Lamar;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Database
{
    public class DatabaseSagaFrameProvider : BaseSagaPersistenceFrameProvider
    {
        public override Frame DetermineStoreOrDeleteFrame(IContainer container, HandlerChain chain,
            MethodCall sagaHandler,
            Variable document,
            Type sagaHandlerType)
        {
            throw new NotImplementedException();
        }

        protected override Frame buildPersistenceFrame(IContainer container, HandlerChain chain,
            SagaStateExistence existence, ref Variable sagaId, Type sagaStateType,
            Variable existingState,
            ref Variable loadedState)
        {
            throw new NotImplementedException();
        }
    }
}
