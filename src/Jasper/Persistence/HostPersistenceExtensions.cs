using Jasper.Persistence.Durability;
using Microsoft.Extensions.DependencyInjection;

#if NETSTANDARD2_0
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
#else
using IHost = Microsoft.Extensions.Hosting.IHost;
#endif

namespace Jasper.Persistence
{
    public static class HostPersistenceExtensions
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
