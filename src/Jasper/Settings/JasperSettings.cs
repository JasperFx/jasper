using System;
using System.Collections.Generic;
using Baseline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jasper.Settings
{
    public class JasperSettings
    {
        private readonly IList<Action<IConfigurationRoot>> _configActions = new List<Action<IConfigurationRoot>>();

        private readonly JasperRegistry _parent;

        private readonly Dictionary<Type, ISettingsBuilder> _settings
            = new Dictionary<Type, ISettingsBuilder>();

        public JasperSettings(JasperRegistry parent)
        {
            _parent = parent;
            Replace<ILoggerFactory>(new LoggerFactory());
        }

        internal bool ApplyingExtensions { get; set; }

        private SettingsBuilder<T> forType<T>(Func<IConfigurationRoot, T> source = null) where T : class
        {
            if (_settings.ContainsKey(typeof(T)))
            {
                var builder = _settings[typeof(T)].As<SettingsBuilder<T>>();

                if (source != null)
                    builder.Replace(source);

                return builder;
            }
            else
            {
                var builder = new SettingsBuilder<T>(source);
                _settings.Add(typeof(T), builder);

                return builder;
            }
        }


        /// <summary>
        ///     Just directs Jasper to try to read data for T from the IConfiguration
        ///     and inject this type into the application container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Require<T>() where T : class, new()
        {
            // Just to register it
            forType<T>();
        }

        /// <summary>
        ///     Configure where a class can find its data, such as a subsection in a file
        /// </summary>
        public void Configure<T>(Func<IConfiguration, IConfiguration> config) where T : class, new()
        {
            forType(root => config(root).Get<T>());
        }

        /// <summary>
        ///     Alter a settings object after it is loaded
        /// </summary>
        public void Alter<T>(Action<T> alteration) where T : class
        {
            var builder = forType<T>();
            if (ApplyingExtensions)
                builder.PackageAlter((_, x) => alteration(x));
            else
                builder.Alter((_, x) => alteration(x));
        }


        public void Alter<T>(Action<IConfigurationRoot, T> alteration) where T : class, new()
        {
            var builder = forType<T>();
            if (ApplyingExtensions)
                builder.PackageAlter(alteration);
            else
                builder.Alter(alteration);
        }

        /// <summary>
        ///     Replace a settings object after it is loaded
        /// </summary>
        public void Replace<T>(T settings) where T : class
        {
            forType<T>().Replace(settings);
        }

        /// <summary>
        ///     Modify the application using loaded settings
        /// </summary>
        public void With<T>(Action<T> alteration) where T : class
        {
            forType<T>().With(alteration);
        }

        public void WithConfig(Action<IConfigurationRoot> configuration)
        {
            _configActions.Add(configuration);
        }


        // TODO -- this will need to have the configuration *pushed* into it from now onw
        internal void Bootstrap()
        {
            // Have ISettingsProvider delegate to JasperSettings

            var config = _parent.Configuration.Build();

            foreach (var configAction in _configActions)
                configAction(config);

            _parent.Services.AddSingleton(config);
            _parent.Services.AddSingleton<IConfiguration>(config);

            foreach (var settings in _settings.Values)
                settings.Apply(config, _parent);
        }


        public void BindToConfigSection<T>(string sectionName) where T : class, new()
        {
            Configure<T>(c => c.GetSection(sectionName));
        }
    }
}
