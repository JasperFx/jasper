using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.SqlServer
{
    /// <summary>
    /// Activates the Sql Server backed message persistence
    /// </summary>
    public class SqlServerMessagePersistence : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<SqlServerSettings>();

            registry.Services.AddSingleton<IDurableMessagingFactory, SqlServerBackedDurableMessagingFactory>();


            // TODO -- more here later
        }
    }
}
