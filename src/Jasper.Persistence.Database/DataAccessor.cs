namespace Jasper.Persistence.Database
{
    public abstract class DataAccessor
    {
        public const string IncomingTable = "jasper_incoming_envelopes";
        public const string OutgoingTable = "jasper_outgoing_envelopes";
        public const string DeadLetterTable = "jasper_dead_letters";
    }
}