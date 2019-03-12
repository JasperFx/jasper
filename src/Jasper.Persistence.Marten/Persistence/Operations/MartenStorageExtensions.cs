using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Marten;
using Marten.Schema;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Postgresql.Util;

namespace Jasper.Persistence.Marten.Persistence.Operations
{
    public static class MartenStorageExtensions
    {

        public static void StoreIncoming(this IDocumentSession session, PostgresqlSettings settings, Envelope envelope)
        {
            var operation = new StoreIncomingEnvelope(settings.IncomingFullName, envelope);
            session.QueueOperation(operation);
        }

        public static void StoreOutgoing(this IDocumentSession session, PostgresqlSettings settings, Envelope envelope,
            int ownerId)
        {
            var operation = new StoreOutgoingEnvelope(settings.OutgoingFullName, envelope, ownerId);
            session.QueueOperation(operation);
        }


    }
}
