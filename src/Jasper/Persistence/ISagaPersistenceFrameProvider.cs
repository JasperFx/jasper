using System;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Lamar;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence
{
    public interface ISagaPersistenceFrameProvider
    {
        Frame DeterminePersistenceFrame(IContainer container, HandlerChain chain, MethodCall sagaHandler,
            SagaStateExistence existence,
            ref Variable sagaId,
            Type sagaStateType,
            Variable existingState, out Variable loadedState);

        Type DetermineSagaIdType(Type sagaStateType);

        Frame DetermineStoreOrDeleteFrame(IContainer container, HandlerChain chain, MethodCall sagaHandler,
            Variable document,
            Type sagaHandlerType);
    }
}
