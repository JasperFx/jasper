using Jasper.Messaging.Durability;

namespace Jasper.Persistence
{
    public static class JasperRuntimeExtensions
    {
        /// <summary>
        ///     Drops and recreates the Sql Server backed persistence database objects
        /// </summary>
        /// <param name="host"></param>
        public static void RebuildMessageStorage(this IJasperHost host)
        {
            host.Get<IEnvelopePersistence>().Admin.RebuildSchemaObjects();
        }

        /// <summary>
        /// Remove any persisted incoming, scheduled, or outgoing message
        /// envelopes from your underlying database
        /// </summary>
        /// <param name="host"></param>
        public static void ClearAllPersistedMessages(this IJasperHost host)
        {
            host.Get<IEnvelopePersistence>().Admin.ClearAllPersistedEnvelopes();
        }


    }
}
