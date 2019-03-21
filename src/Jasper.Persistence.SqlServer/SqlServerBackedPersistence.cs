using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Util;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.SqlServer
{
    /// <summary>
    ///     Activates the Sql Server backed message persistence
    /// </summary>
    public class SqlServerBackedPersistence : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<SqlServerSettings>();

            registry.Services.AddTransient<IEnvelopePersistence, SqlServerEnvelopePersistence>();

            registry.CodeGeneration.Sources.Add(new DatabaseBackedPersistenceMarker());


            registry.Services.For<SqlConnection>().Use<SqlConnection>();

            registry.Services.Add(new SqlConnectionInstance(typeof(SqlConnection)));
            registry.Services.Add(new SqlConnectionInstance(typeof(DbConnection)));

            registry.CodeGeneration.Transactions = new SqlServerTransactionFrameProvider();
        }
    }
}
