using System;
using Jasper.Messaging.Sagas;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Marten;
using Marten.Schema;
using Marten.Util;

namespace Jasper.Marten.Persistence
{
    // TODO -- need to get this onto HandlerGraph
    public class MartenSagaPersistence : ISagaPersistence
    {

        public Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType)
        {
            throw new NotImplementedException();
        }

        public Type DetermineSagaIdType(Type sagaStateType)
        {
            var mapping = new DocumentMapping(sagaStateType, new StoreOptions());
            return mapping.IdMember.GetMemberType();
        }
    }
}
