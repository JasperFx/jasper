using System;
using System.Collections.Generic;
using Baseline;
using Lamar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Jasper.Settings
{
    public interface IHasRegistryParent
    {
        JasperRegistry Parent { get; }
    }

    public class JasperSettings : IHasRegistryParent
    {
        public static string ConfigSectionNameFor(Type type)
        {
            if (type.Name.EndsWith("Settings")) return type.Name.Substring(0, type.Name.Length - 8);
            if (type.Name.EndsWith("Options"))return type.Name.Substring(0, type.Name.Length - 7);

            return type.Name;
        }

        private readonly IList<Action<WebHostBuilderContext>>
            _configActions = new List<Action<WebHostBuilderContext>>();

        private readonly JasperRegistry _parent;

        private readonly Dictionary<Type, ISettingsBuilder> _settings
            = new Dictionary<Type, ISettingsBuilder>();

        public JasperSettings(JasperRegistry parent)
        {
            _parent = parent;
        }

        internal bool ApplyingExtensions { private get; set; }

        JasperRegistry IHasRegistryParent.Parent => _parent;

        private SettingsBuilder<T> forType<T>(Func<WebHostBuilderContext, T> source = null) where T : class
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
            forType(root => config(root.Configuration).Get<T>());
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

        /// <summary>
        ///     Alter a settings object after it has been loaded from configuration using
        ///     the IConfiguration and IHostingEnvironment for the application
        /// </summary>
        /// <param name="alteration"></param>
        /// <typeparam name="T"></typeparam>
        public void Alter<T>(Action<WebHostBuilderContext, T> alteration) where T : class, new()
        {
            var builder = forType<T>();
            if (ApplyingExtensions)
                builder.PackageAlter(alteration);
            else
                builder.Alter(alteration);
        }

        /// <summary>
        ///     Alter advanced features of the messaging support
        /// </summary>
        /// <param name="alteration"></param>
        public void Messaging(Action<JasperOptions> alteration)
        {
            Alter(alteration);
        }

        /// <summary>
        ///     Alter advanced features of the messaging support with access to the
        ///     IConfiguration and IHostingEnvironment for the application
        /// </summary>
        /// <param name="alteration"></param>
        public void Messaging(Action<WebHostBuilderContext, JasperOptions> alteration)
        {
            Alter(alteration);
        }

        /// <summary>
        ///     Replace a settings object after it is loaded
        /// </summary>
        public void Replace<T>(T settings) where T : class
        {
            forType<T>().Replace(settings);
        }

        /// <summary>
        ///     Apply additional changes to this JasperRegistry object based on
        ///     the loaded IConfiguration and IHostedEnvironment for the application
        /// </summary>
        /// <param name="configuration"></param>
        public void Configure(Action<WebHostBuilderContext> configuration)
        {
            _configActions.Add(configuration);
        }


        /// <summary>
        ///     Load a settings object "T" from the named configuration section
        /// </summary>
        /// <param name="sectionName"></param>
        /// <typeparam name="T"></typeparam>
        public void BindToConfigSection<T>(string sectionName) where T : class, new()
        {
            Configure<T>(c => c.GetSection(sectionName));
        }

        internal void Apply(ServiceRegistry services)
        {
            foreach (var setting in _settings)
            {
                setting.Value.Apply(services);
            }
        }
    }
}
