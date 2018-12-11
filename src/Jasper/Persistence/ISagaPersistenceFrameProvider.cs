using System;
using Jasper.Messaging.Sagas;
using LamarCompiler.Frames;
using LamarCompiler.Model;

namespace Jasper.Persistence
{
    public interface ISagaPersistenceFrameProvider
    {
        Frame DeterminePersistenceFrame(SagaStateExistence existence, ref Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState);

        Type DetermineSagaIdType(Type sagaStateType);

        Frame DetermineStoreOrDeleteFrame(Variable document, Type sagaHandlerType);
    }
}
