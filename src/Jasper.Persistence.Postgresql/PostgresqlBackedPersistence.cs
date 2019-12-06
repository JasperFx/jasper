using System.Data.Common;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Persistence.Database;
using Jasper.Settings;
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
        public void Configure(JasperOptions options)
        {
            options.Services.AddSingleton(Settings);

            options.Services.AddTransient<IEnvelopePersistence, PostgresqlEnvelopePersistence>();

            options.CodeGeneration.Sources.Add(new DatabaseBackedPersistenceMarker());


            options.Services.For<NpgsqlConnection>().Use<NpgsqlConnection>();

            options.Services.Add(new NpgsqlConnectionInstance(typeof(NpgsqlConnection)));
            options.Services.Add(new NpgsqlConnectionInstance(typeof(DbConnection)));

            options.CodeGeneration.SetTransactions(new PostgresqlTransactionFrameProvider());
        }

        public PostgresqlSettings Settings { get; } = new PostgresqlSettings();
    }
}
