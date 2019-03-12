namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlSettings : PostgresqlAccess
    {
        private string _schemaName;

        public PostgresqlSettings()
        {
            SchemaName = "public";
        }

        public string ConnectionString { get; set; }

        public string SchemaName
        {
            get => _schemaName;
            set
            {
                _schemaName = value;

                IncomingFullName = $"{value}.{IncomingTable}";
                OutgoingFullName = $"{value}.{OutgoingTable}";
                DeadLetterFullName = $"{value}.{DeadLetterTable}";
            }
        }

        public string DeadLetterFullName { get; private set; }

        public string OutgoingFullName { get; private set; }

        public string IncomingFullName { get; private set; }
    }

    public abstract class PostgresqlAccess
    {
        public const string IncomingTable = "jasper_incoming_envelopes";
        public const string OutgoingTable = "jasper_outgoing_envelopes";
        public const string DeadLetterTable = "jasper_dead_letters";
    }
}
