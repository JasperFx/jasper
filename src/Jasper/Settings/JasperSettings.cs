using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Baseline;
using Microsoft.Extensions.Configuration;
using Jasper.Configuration;

namespace Jasper.Settings
{
    public class JasperSettings
    {
        private readonly IConfigurationBuilder _builder;
        private readonly IList<ISettingsAlteration> _alterations = new List<ISettingsAlteration>();
        private readonly IList<ISettingsConfiguration> _settingsConfigurations = new List<ISettingsConfiguration>();
        private readonly SettingsCollection _settingsCollection = new SettingsCollection();
        private Action<IConfigurationBuilder> _configBuilder;

        public JasperSettings()
        {
            _configBuilder = build;
            _builder = new ConfigurationBuilder();
        }


        public void Build(Action<IConfigurationBuilder> build)
        {
            _configBuilder = build;
        }

        public void Configure<T>() where T : class, new()
        {
            Configure<T>(x => { });
        }

        public void Configure<T>(Action<IConfiguration> config) where T : class, new()
        {
            _settingsConfigurations.Fill(new SettingsConfiguration<T>(config));
        }

        public void Alter<T>(Action<T> alteration) where T : class, new()
        {
            _alterations.Fill(new SettingAlteration<T>(alteration));
        }

        public void Replace<T>(T settings) where T : class
        {
            _alterations.Fill(new SettingReplacement<T>(settings));
        }

        public T Get<T>() where T : class, new()
        {
            return _settingsCollection.Get<T>();
        }

        public void Bootstrap(ServiceRegistry registry)
        {
            _configBuilder(_builder);
            var configuration = _builder.Build();

            _settingsConfigurations.Each(config =>
            {
                var instance = config.Configure(configuration);
                _settingsCollection.Add(instance);
            });

            _alterations.Each(alteration => alteration.Alter(_settingsCollection));

            _settingsCollection.Register(registry);
        }

        private void build(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json");
        }
    }
}