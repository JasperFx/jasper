using System;
using Jasper.Messaging.Sagas;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Persistence
{
    public interface ISagaPersistenceFrameProvider
    {
        Frame DeterminePersistenceFrame(MethodCall sagaHandler, SagaStateExistence existence, ref Variable sagaId,
            Type sagaStateType,
            Variable existingState, out Variable loadedState);

        Type DetermineSagaIdType(Type sagaStateType);

        Frame DetermineStoreOrDeleteFrame(MethodCall sagaHandler, Variable document, Type sagaHandlerType);
    }
}
