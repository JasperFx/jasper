using System;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas;

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
