using System;
using System.Collections.Generic;
using Baseline;
using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public class JasperSettings
    {
        private readonly JasperRegistry _registry;
        private readonly IList<IRegistryAlteration> _registryAlterations = new List<IRegistryAlteration>();
        private readonly IList<ISettingsAlteration> _settingsAlterations = new List<ISettingsAlteration>();
        private readonly SettingsProvider _settingsProvider;

        public JasperSettings(JasperRegistry registry)
        {
            _registry = registry;
            _settingsProvider = new SettingsProvider();
        }

        /// <summary>
        ///     Add additional configuration sources
        /// </summary>
        public void Build(Action<IConfigurationBuilder> build)
        {
            _settingsProvider.Builder = build;
        }

        /// <summary>
        ///     Add a class to settings that does not follow the convention of ending with "Settings"
        /// </summary>
        public void Configure<T>() where T : class, new()
        {
            _registry.Services.ForSingletonOf(typeof(T))
                .Use(context => context.GetInstance<ISettingsProvider>().Get<T>());
        }

        /// <summary>
        ///     Configure where a class can find its data, such as a subsection in a file
        /// </summary>
        public void Configure<T>(Func<IConfiguration, IConfiguration> config) where T : class, new()
        {
            _settingsProvider.Configure<T>(config);
            Configure<T>();
        }

        /// <summary>
        ///     Alter a settings object after it is loaded
        /// </summary>
        public void Alter<T>(Action<T> alteration) where T : class, new()
        {
            _settingsAlterations.Fill(new SettingAlteration<T>(alteration));
        }

        /// <summary>
        ///     Replace a settings object after it is loaded
        /// </summary>
        public void Replace<T>(T settings) where T : class, new()
        {
            _settingsAlterations.Fill(new SettingReplacement<T>(settings));
        }

        /// <summary>
        ///     Modify the application using loaded settings
        /// </summary>
        public void With<T>(Action<T> alteration) where T : class, new()
        {
            _registryAlterations.Fill(new RegistryAlteration<T>(alteration));
        }

        public T Get<T>() where T : class, new()
        {
            return _settingsProvider.Get<T>();
        }

        public void Bootstrap()
        {
            _registry.Services.ForSingletonOf<ISettingsProvider>().Use(_settingsProvider);
            _registry.Services.Policies.OnMissingFamily<SettingsPolicy>();

            _settingsAlterations.Each(alteration => alteration.Alter(_settingsProvider));
            _registryAlterations.Each(alteration => alteration.Alter(_settingsProvider));
        }
    }
}