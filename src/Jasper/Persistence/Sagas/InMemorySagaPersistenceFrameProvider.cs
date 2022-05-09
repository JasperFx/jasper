using System;
using Jasper.Runtime.Handlers;
using Lamar;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Persistence.Sagas;

public class InMemorySagaPersistenceFrameProvider : BaseSagaPersistenceFrameProvider
{
    public override Frame DetermineStoreOrDeleteFrame(IContainer container, HandlerChain chain,
        MethodCall sagaHandler,
        Variable document,
        Type sagaHandlerType)
    {
        return new StoreOrDeleteSagaStateFrame(document, sagaHandlerType);
    }

    protected override Frame buildPersistenceFrame(IContainer container, HandlerChain chain,
        SagaStateExistence existence, ref Variable sagaId, Type sagaStateType,
        Variable existingState,
        ref Variable loadedState)
    {
        var frame = new InMemorySagaPersistenceFrame(sagaStateType, sagaId, existence);

        if (existence == SagaStateExistence.Existing)
        {
            loadedState = frame.Document;
        }

        return frame;
    }
}
