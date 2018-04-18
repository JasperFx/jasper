using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.DbObjects;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Marten;
using Marten.Schema;
using Marten.Util;
using Npgsql;

namespace Jasper.Marten.Persistence.Operations
{
    public static class MartenStorageExtensions
    {
        public static async Task<List<Envelope>> ExecuteToEnvelopes(this NpgsqlCommand command)
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                var list = new List<Envelope>();

                while (await reader.ReadAsync())
                {
                    var bytes = await reader.GetFieldValueAsync<byte[]>(0);
                    var envelope = Envelope.Read(bytes);

                    list.Add(envelope);
                }

                return list;
            }
        }

        public static List<Envelope> LoadEnvelopes(this NpgsqlCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                var list = new List<Envelope>();

                while (reader.Read())
                {
                    var bytes = reader.GetFieldValue<byte[]>(0);
                    var envelope = Envelope.Read(bytes);
                    envelope.Status = reader.GetFieldValue<string>(1);
                    envelope.OwnerId = reader.GetFieldValue<int>(2);


                    if (!reader.IsDBNull(3))
                    {
                        var raw = reader.GetFieldValue<DateTime>(3);


                        envelope.ExecutionTime = raw.ToUniversalTime();
                    }

                    // Attempts will come in from the Envelope.Read
                    //envelope.Attempts = reader.GetFieldValue<int>(4);

                    list.Add(envelope);
                }

                return list;
            }
        }

        public static List<Envelope> AllIncomingEnvelopes(this IQuerySession session)
        {
            var schema = session.DocumentStore.Tenancy.Default.DbObjects.SchemaTables()
                .FirstOrDefault(x => x.Name == PostgresqlEnvelopeStorage.IncomingTableName).Schema;



            return session.Connection
                .CreateCommand($"select body, status, owner_id, execution_time, attempts from {schema}.{PostgresqlEnvelopeStorage.IncomingTableName}")
                .LoadEnvelopes();
        }

        public static void DeleteEnvelopes(this IDocumentSession session, DbObjectName table,
            IEnumerable<Envelope> envelopes)
        {
            var operation = new DeleteEnvelopes(table, envelopes);
            session.QueueOperation(operation);
        }


        public static List<Envelope> AllOutgoingEnvelopes(this IQuerySession session)
        {
            var schema = session.DocumentStore.Tenancy.Default.DbObjects.SchemaTables()
                .FirstOrDefault(x => x.Name == PostgresqlEnvelopeStorage.IncomingTableName).Schema;



            return session.Connection
                .CreateCommand($"select body, '{TransportConstants.Outgoing}', owner_id, now() as execution_time, 0 from {schema}.{PostgresqlEnvelopeStorage.OutgoingTableName}")
                .LoadEnvelopes();
        }

        public static void StoreIncoming(this IDocumentSession session, EnvelopeTables marker, Envelope envelope)
        {
            var operation = new StoreIncomingEnvelope(marker.Incoming, envelope);
            session.QueueOperation(operation);
        }

        public static void StoreOutgoing(this IDocumentSession session, EnvelopeTables marker, Envelope envelope, int ownerId)
        {
            var operation = new StoreOutgoingEnvelope(marker.Outgoing, envelope, ownerId);
            session.QueueOperation(operation);
        }

        public static void StoreIncoming(this IDocumentSession session, EnvelopeTables marker, Envelope[] messages)
        {
            foreach (var envelope in messages)
            {
                var operation = new StoreIncomingEnvelope(marker.Incoming, envelope);
                session.QueueOperation(operation);
            }
        }

        public static void MarkOwnership(this IDocumentSession session, DbObjectName table, int ownerId,
            IEnumerable<Envelope> envelopes)
        {
            var operation = new MarkOwnership(table, ownerId, envelopes);
            session.QueueOperation(operation);
        }

        public static void DeleteEnvelopes(this IDocumentSession session, DbObjectName table, Guid[] idlist)
        {
            var operation = new DeleteEnvelopes(table, idlist);
            session.QueueOperation(operation);
        }

        public static void ScheduleExecution(this IDocumentSession session, DbObjectName table, Envelope envelope)
        {
            var operation = new ScheduleEnvelope(table, envelope);
            session.QueueOperation(operation);
        }
    }
}
