﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        private readonly IList<IRegistryAlteration> _registryAlterations = new List<IRegistryAlteration>();
        private readonly IList<ISettingsAlteration> _settingsAlterations = new List<ISettingsAlteration>();
        private readonly SettingsProvider _settingsProvider;
        private readonly JasperRegistry _registry;

        public JasperSettings(JasperRegistry registry)
        {
            _registry = registry;
            _settingsProvider = new SettingsProvider();
        }


        public void Build(Action<IConfigurationBuilder> build)
        {
            _settingsProvider.Builder = build;
        }

        public void Configure<T>() where T : class, new()
        {
            _registry.Services.ForSingletonOf(typeof(T)).Use(context => context.GetInstance<ISettingsProvider>().Get<T>());
        }

        public void Configure<T>(Func<IConfiguration, IConfiguration> config) where T : class, new()
        {
            _settingsProvider.Configure<T>(config);
            Configure<T>();
        }

        public void Alter<T>(Action<T> alteration) where T : class, new()
        {
            _settingsAlterations.Fill(new SettingAlteration<T>(alteration));
        }

        public void Replace<T>(T settings) where T : class, new()
        {
            _settingsAlterations.Fill(new SettingReplacement<T>(settings));
        }

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

            _registryAlterations.Each(alteration => alteration.Alter(_settingsProvider));
            _settingsAlterations.Each(alteration => alteration.Alter(_settingsProvider));
        }
    }
}