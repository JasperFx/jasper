using System;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;

namespace Jasper.Messaging.Sagas
{
    public interface ISagaPersistence
    {
        Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            Variable existingState);
        Type DetermineSagaIdType(Type sagaStateType);
    }
}
