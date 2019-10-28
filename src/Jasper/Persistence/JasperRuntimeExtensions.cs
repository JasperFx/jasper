using Jasper.Messaging.Durability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence
{
    public static class JasperRuntimeExtensions
    {

        /// <summary>
        ///     Drops and recreates the Sql Server backed persistence database objects
        /// </summary>
        /// <param name="host"></param>
        public static void RebuildMessageStorage(this IHost host)
        {
            host.Services.GetRequiredService<IEnvelopePersistence>().Admin.RebuildSchemaObjects();
        }

        /// <summary>
        /// Remove any persisted incoming, scheduled, or outgoing message
        /// envelopes from your underlying database
        /// </summary>
        /// <param name="host"></param>
        public static void ClearAllPersistedMessages(this IHost host)
        {
            host.Services.GetRequiredService<IEnvelopePersistence>().Admin.ClearAllPersistedEnvelopes();
        }

    }
}
