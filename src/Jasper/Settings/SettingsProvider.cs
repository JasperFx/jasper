using System;
using System.Collections.Concurrent;
using Baseline;
using Jasper.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public interface ISettingsProvider
    {
        T Get<T>() where T : class, new();
        void Alter<T>(Action<T> alteration) where T : class, new();
        void Replace<T>(T settings) where T : class;
    }

    public class SettingsProvider : ISettingsProvider
    {
        private readonly Lazy<IConfigurationRoot> _configuration;
        private readonly ConcurrentDictionary<Type, object> _settings = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, ISettingsConfiguration> _configurations = new ConcurrentDictionary<Type, ISettingsConfiguration>();

        public IConfigurationRoot Configuration => _configuration.Value;

        public Action<IConfigurationBuilder> Builder { get; set; } = _ => { };

        public SettingsProvider()
        {
            _configuration = new Lazy<IConfigurationRoot>(() =>
            {
                var builder = new ConfigurationBuilder();
                Builder(builder);
                return builder.Build();
            });
        }

        public T Get<T>() where T : class, new()
        {
            return _settings.GetOrAdd(typeof(T), t => GetValue<T>()).As<T>();
        }

        public void Alter<T>(Action<T> alteration) where T : class, new()
        {
            var value = Get<T>();
            alteration(value);
            _settings.AddOrUpdate(typeof(T), t => value, (x, y) => value);
        }

        public void AlterRegistry<T>(Action<T> alteration) where T : class, new()
        {
            var value = Get<T>();
            alteration(value);
        }

        public void Replace<T>(T settings) where T : class
        {
            _settings.AddOrUpdate(typeof(T), t => settings, (x, y) => settings);
        }

        public void Configure<T>(Func<IConfiguration, IConfiguration> config) where T : class, new()
        {
            _configurations.TryAdd(typeof(T), new SettingsConfiguration(config));
        }

        private T GetValue<T>() where T : class, new()
        {
            var type = typeof(T);
            T value;
            if (_configurations.ContainsKey(type))
            {
                value = _configurations[type].Configure(_configuration.Value).Get<T>();
            }
            else
            {
                value = _configuration.Value.Get<T>();
            }

            return value ?? new T();
        }
    }
}