using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using StructureMap;

namespace Jasper.Settings
{
    public class JasperSettings
    {
        private readonly Dictionary<Type, ISettingsBuilder> _settings
            = new Dictionary<Type, ISettingsBuilder>();

        private readonly JasperRegistry _parent;
        private readonly IList<Action<IConfigurationRoot>> _configActions = new List<Action<IConfigurationRoot>>();

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
                var builder =  _settings[typeof(T)].As<SettingsBuilder<T>>();

                if (source != null)
                {
                    builder.Replace(source);
                }

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
        ///     Add a class to settings that does not follow the convention of ending with "Settings"
        /// </summary>
        [Obsolete("Might eliminate or just rename")]
        public void Configure<T>() where T : class, new()
        {
            // Just to register it
            forType<T>();
        }

        /// <summary>
        ///     Configure where a class can find its data, such as a subsection in a file
        /// </summary>
        public void Configure<T>(Func<IConfiguration, IConfiguration> config) where T : class, new()
        {
            forType<T>(root => config(root).Get<T>());
        }

        /// <summary>
        ///     Alter a settings object after it is loaded
        /// </summary>
        public void Alter<T>(Action<T> alteration) where T : class
        {
            var builder = forType<T>();
            if (ApplyingExtensions)
            {
                builder.PackageAlter((_, x) => alteration(x));
            }
            else
            {
                builder.Alter((_, x) => alteration(x));
            }
        }


        public void Alter<T>(Action<IConfigurationRoot, T> alteration) where T : class, new()
        {
            var builder = forType<T>();
            if (ApplyingExtensions)
            {
                builder.PackageAlter(alteration);
            }
            else
            {
                builder.Alter(alteration);
            }
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

        internal T Get<T>() where T : class, new()
        {
            throw new NotImplementedException("Used for testing, just do it end to end now");
        }

        internal void Bootstrap()
        {
            // Have ISettingsProvider delegate to JasperSettings

            var config = _parent.Configuration.Build();

            foreach (var configAction in _configActions)
            {
                configAction(config);
            }

            _parent.Services.ForSingletonOf<IConfigurationRoot>().Use(config);

            foreach (var settings in _settings.Values)
            {
                settings.Apply(config, _parent);
            }
        }


        public void BindToConfigSection<T>(string sectionName) where T : class, new()
        {
            Configure<T>(c => c.GetSection(sectionName));
        }


    }
}
