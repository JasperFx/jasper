using System.Data.Common;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Persistence.Database;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Jasper.Persistence.Postgresql
{
    /// <summary>
    ///     Activates the Sql Server backed message persistence
    /// </summary>
    public class PostgresqlBackedPersistence : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<PostgresqlSettings>();

            registry.Services.AddTransient<IEnvelopePersistence, PostgresqlEnvelopePersistence>();

            registry.CodeGeneration.Sources.Add(new DatabaseBackedPersistenceMarker());


            registry.Services.For<NpgsqlConnection>().Use<NpgsqlConnection>();

            registry.Services.Add(new NpgsqlConnectionInstance(typeof(NpgsqlConnection)));
            registry.Services.Add(new NpgsqlConnectionInstance(typeof(DbConnection)));

            registry.CodeGeneration.Transactions = new PostgresqlTransactionFrameProvider();
        }
    }
}
