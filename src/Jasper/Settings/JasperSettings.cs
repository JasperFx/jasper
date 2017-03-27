using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Baseline;
using Microsoft.Extensions.Configuration;
using Jasper.Configuration;
using StructureMap.Graph.Scanning;
using StructureMap.TypeRules;

namespace Jasper.Settings
{
    public class JasperSettings
    {
        private const string defaultConfigFileName = "appsettings.config";

        private readonly IConfigurationBuilder _builder;
        private readonly IList<ISettingsAlteration> _alterations = new List<ISettingsAlteration>();
        private readonly IList<ISettingsConfiguration> _configurations = new List<ISettingsConfiguration>();
        private readonly SettingsCollection _settingsCollection = new SettingsCollection();
        private Action<IConfigurationBuilder> _configBuilder;
        private readonly JasperRegistry _registry;

        public JasperSettings(JasperRegistry registry)
        {
            _configBuilder = build;
            _registry = registry;
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
            _configurations.Fill(new SettingsConfiguration<T>(config));
        }

        public void Alter<T>(Action<T> alteration) where T : class, new()
        {
            _alterations.Fill(new SettingAlteration<T>(alteration));
        }

        public void Replace<T>(T settings) where T : class, new()
        {
            _alterations.Fill(new SettingReplacement<T>(settings));
        }

        public void With<T>(Action<T> alteration) where T : class, new() 
        {
            _alterations.Fill(new RegistryAlteration<T>(alteration));
        }

        public T Get<T>() where T : class, new()
        {
            return _settingsCollection.Get<T>();
        }

        public void Bootstrap()
        {
            _configBuilder(_builder);
            var configuration = _builder.Build();
            _registry.Services.ForSingletonOf<IConfigurationRoot>().Use(configuration);
            _registry.Services.Policies.OnMissingFamily<SettingsPolicy>();

            _configurations.Each(config =>
            {
                var instance = config.Configure(configuration);
                _settingsCollection.Add(instance);
            });

            _alterations.Each(alteration => alteration.Alter(_settingsCollection));

            _settingsCollection.Register(_registry.Services);
        }

        private void build(IConfigurationBuilder builder)
        {
            builder.AddJsonFile(defaultConfigFileName);
        }
    }
}