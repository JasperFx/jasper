using System;
using System.Collections.Generic;
using System.IO;
using Baseline;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Marten;
using Marten.Schema;
using Marten.Storage;
using Marten.Util;

namespace Jasper.Persistence.Marten.Persistence
{
    public class MartenEnvelopeAdmin : IEnvelopeStorageAdmin
    {
        private readonly IDocumentStore _store;
        private readonly EnvelopeTables _tables;

        public MartenEnvelopeAdmin(IDocumentStore store, EnvelopeTables tables)
        {
            _store = store;
            _tables = tables;
        }

        public void ClearAllPersistedEnvelopes()
        {
            using (var session = _store.LightweightSession())
            {
                session.Connection.CreateCommand($"delete from {_tables.Incoming};delete from {_tables.Outgoing}")
                    .ExecuteNonQuery();


            }

            _store.Advanced.Clean.DeleteDocumentsFor(typeof(ErrorReport));
        }

        public void RebuildSchemaObjects()
        {
            _store.Advanced.Clean.CompletelyRemove(typeof(Envelope));
            _store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));

            _store.Advanced.Clean.CompletelyRemove(typeof(ErrorReport));
            _store.Tenancy.Default.EnsureStorageExists(typeof(ErrorReport));

        }

        public string CreateSql()
        {
            var documentStore = _store.As<DocumentStore>();

            var writer = new StringWriter();

            var options = documentStore.Options;
            new SchemaPatch(options.DdlRules).WriteScript(writer, w =>
            {
                var allSchemaNames = options.Storage.AllSchemaNames();
                DatabaseSchemaGenerator.WriteSql(options, allSchemaNames, w);

                var envelopes = options.Storage.FindFeature(typeof(Envelope));

                envelopes.Write(options.DdlRules, writer);

                var errorReports = options.Storage.MappingFor(typeof(ErrorReport));
                errorReports.Write(options.DdlRules, writer);
            }, true);

            return writer.ToString();
        }

    }
}
