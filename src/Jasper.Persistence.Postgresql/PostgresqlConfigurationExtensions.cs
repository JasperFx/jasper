using System;
using System.Linq;
using Baseline;
using Jasper.Settings;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.Postgresql
{
    public static class PostgresqlConfigurationExtensions
    {
        /// <summary>
        ///     Register sql server backed message persistence to a known connection string
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="connectionString"></param>
        /// <param name="schema"></param>
        public static void PersistMessagesWithPostgresql(this IExtensions extensions, string connectionString,
            string schema = null)
        {
            extensions.Include<PostgresqlBackedPersistence>(o =>
            {
                o.Settings.ConnectionString = connectionString;
                if (schema.IsNotEmpty())
                {
                    o.Settings.SchemaName = schema;
                }
            });
        }

    }
}
