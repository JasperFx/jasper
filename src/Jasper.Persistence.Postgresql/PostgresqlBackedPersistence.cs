using System.Data.Common;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Sagas;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Weasel.Core.Migrations;

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
            options.Services.AddSingleton(s => (IDatabase)s.GetRequiredService<IEnvelopePersistence>());
            options.Advanced.CodeGeneration.Sources.Add(new DatabaseBackedPersistenceMarker());

            options.Services.For<NpgsqlConnection>().Use<NpgsqlConnection>();

            options.Services.Add(new ServiceDescriptor(typeof(NpgsqlConnection),new NpgsqlConnectionInstance(typeof(NpgsqlConnection))));
            options.Services.Add(new ServiceDescriptor(typeof(DbConnection), new NpgsqlConnectionInstance(typeof(DbConnection))));

            options.Advanced.CodeGeneration.SetTransactionsIfNone(new PostgresqlTransactionFrameProvider());
        }

        public PostgresqlSettings Settings { get; } = new PostgresqlSettings();
    }
}
