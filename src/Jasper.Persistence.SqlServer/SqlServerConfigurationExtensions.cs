using System;
using System.Linq;
using Baseline;
using Jasper.Settings;
using Microsoft.Extensions.Hosting;

namespace Jasper.Persistence.SqlServer
{
    public static class SqlServerConfigurationExtensions
    {
        /// <summary>
        ///     Register sql server backed message persistence to a known connection string
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connectionString"></param>
        /// <param name="schema"></param>
        public static void PersistMessagesWithSqlServer(this SettingsGraph settings, string connectionString,
            string schema = null)
        {
            var parent = settings.As<IHasRegistryParent>().Parent;
            if (!parent.AppliedExtensions.OfType<SqlServerBackedPersistence>().Any())
                parent.Extensions.Include<SqlServerBackedPersistence>();

            settings.Alter<SqlServerSettings>(x =>
            {
                x.ConnectionString = connectionString;
                if (schema.IsNotEmpty()) x.SchemaName = schema;
            });
        }

        /// <summary>
        ///     Register sql server backed message persistence based on configuration and the
        ///     development environment
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void PersistMessagesWithSqlServer(this SettingsGraph settings,
            Action<HostBuilderContext, SqlServerSettings> configure)
        {
            var parent = settings.As<IHasRegistryParent>().Parent;
            if (!parent.AppliedExtensions.OfType<SqlServerBackedPersistence>().Any())
                parent.Extensions.Include<SqlServerBackedPersistence>();

            settings.Alter(configure);
        }
    }
}
