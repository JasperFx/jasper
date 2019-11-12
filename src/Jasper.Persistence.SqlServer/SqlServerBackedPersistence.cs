using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Settings;
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
        public void Configure(JasperOptions options)
        {
            options.Settings.Require<SqlServerSettings>();

            options.Services.AddTransient<IEnvelopePersistence, SqlServerEnvelopePersistence>();

            options.CodeGeneration.Sources.Add(new DatabaseBackedPersistenceMarker());


            options.Services.For<SqlConnection>().Use<SqlConnection>();

            options.Services.Add(new SqlConnectionInstance(typeof(SqlConnection)));
            options.Services.Add(new SqlConnectionInstance(typeof(DbConnection)));

            options.CodeGeneration.SetTransactions(new SqlServerTransactionFrameProvider());
        }
    }
}
