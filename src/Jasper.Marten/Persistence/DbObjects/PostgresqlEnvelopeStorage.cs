using System;
using System.Collections.Generic;
using Jasper.Messaging.Runtime;
using Marten;
using Marten.Storage;

namespace Jasper.Marten.Persistence.DbObjects
{
    public class PostgresqlEnvelopeStorage : FeatureSchemaBase
    {
        public const string IncomingTableName = "mt_envelopes_incoming";
        public const string OutgoingTableName = "mt_envelopes_outgoing";

        public PostgresqlEnvelopeStorage(StoreOptions options) : base("envelope-storage", options)
        {
        }

        protected override IEnumerable<ISchemaObject> schemaObjects()
        {
            yield return new IncomingEnvelopeTable(Options);
            yield return new OutgoingEnvelopeTable(Options);
        }

        public override Type StorageType => typeof(Envelope);

        public override bool IsActive(StoreOptions options)
        {
            return true;
        }
    }
}
