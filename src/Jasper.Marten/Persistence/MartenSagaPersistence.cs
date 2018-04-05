using System;
using Jasper.Marten.Codegen;
using Jasper.Messaging.Sagas;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Marten;
using Marten.Schema;
using Marten.Util;

namespace Jasper.Marten.Persistence
{
    public class MartenSagaPersistence : ISagaPersistence
    {

        public Frame DeterminePersistenceFrame(SagaStateExistence existence, Variable sagaId, Type sagaStateType,
            Variable existingState, out Variable loadedState)
        {


            var frame = new TransactionalFrame();
            if (existence == SagaStateExistence.Existing)
            {
                var doc = frame.LoadDocument(sagaStateType, sagaId);
                loadedState = doc;

                if (existingState == null)
                {
                    frame.SaveDocument(doc);
                }
                else
                {
                    frame.SaveDocument(existingState);
                }
            }
            else
            {
                loadedState = existingState;
                frame.SaveDocument(existingState);
            }

            return frame;
        }

        public Type DetermineSagaIdType(Type sagaStateType)
        {
            var mapping = new DocumentMapping(sagaStateType, new StoreOptions());
            return mapping.IdMember.GetMemberType();
        }
    }
}
