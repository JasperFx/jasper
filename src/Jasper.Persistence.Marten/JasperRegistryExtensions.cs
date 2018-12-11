using System;
using System.Linq;
using Baseline;
using Jasper.Settings;
using Marten;
using Microsoft.AspNetCore.Hosting;

namespace Jasper.Persistence.Marten
{
    public static class JasperRegistryExtensions
    {
        public static void MartenConnectionStringIs(this JasperSettings settings, string connectionString)
        {
            settings.Alter<StoreOptions>(x => x.Connection(connectionString));
        }

        public static void MartenConnectionStringIs(this JasperRegistry registry, string connectionString)
        {
            registry.Settings.MartenConnectionStringIs(connectionString);
        }

        public static void ConfigureMarten(this JasperRegistry registry, Action<StoreOptions> configuration)
        {
            registry.Settings.ConfigureMarten(configuration);
        }

        public static void ConfigureMarten(this JasperSettings settings, Action<StoreOptions> configuration)
        {
            settings.Alter(configuration);
        }

        public static void ConfigureMarten(this JasperSettings settings,
            Action<WebHostBuilderContext, StoreOptions> configuration)
        {
            settings.Alter(configuration);
        }

        /// <summary>
        ///     Register Marten backed message persistence to a known connection string
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connectionString"></param>
        /// <param name="schema"></param>
        public static void PersistMessagesWithMarten(this JasperSettings settings, string connectionString,
            string schema = null)
        {
            var parent = settings.As<IHasRegistryParent>().Parent;
            if (!parent.AppliedExtensions.OfType<MartenBackedPersistence>().Any())
                parent.Include<MartenBackedPersistence>();

            settings.Alter<StoreOptions>(x => { x.Connection(connectionString); });
        }

        /// <summary>
        ///     Register Marten backed message persistence based on configuration and the
        ///     development environment
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configure"></param>
        public static void PersistMessagesWithMarten(this JasperSettings settings,
            Action<WebHostBuilderContext, StoreOptions> configure)
        {
            var parent = settings.As<IHasRegistryParent>().Parent;
            if (!parent.AppliedExtensions.OfType<MartenBackedPersistence>().Any())
                parent.Include<MartenBackedPersistence>();

            settings.Alter(configure);
        }
    }
}
