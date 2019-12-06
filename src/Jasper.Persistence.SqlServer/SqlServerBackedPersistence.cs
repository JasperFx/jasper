using System.Data.Common;
using System.Data.SqlClient;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Util;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.SqlServer
{
    /// <summary>
    ///     Activates the Sql Server backed message persistence
    /// </summary>
    public class SqlServerBackedPersistence : IJasperExtension
    {
        public SqlServerSettings Settings { get; } = new SqlServerSettings();

        public void Configure(JasperOptions options)
        {
            options.Services.AddSingleton(Settings);

            options.Services.AddTransient<IEnvelopePersistence, SqlServerEnvelopePersistence>();

            options.CodeGeneration.Sources.Add(new DatabaseBackedPersistenceMarker());


            options.Services.For<SqlConnection>().Use<SqlConnection>();

            options.Services.Add(new SqlConnectionInstance(typeof(SqlConnection)));
            options.Services.Add(new SqlConnectionInstance(typeof(DbConnection)));

            options.CodeGeneration.SetTransactions(new SqlServerTransactionFrameProvider());
        }
    }
}
