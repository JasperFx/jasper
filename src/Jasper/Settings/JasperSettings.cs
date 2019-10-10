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

    [Obsolete("Trying to replace with teh IOptions model")]
    public class JasperSettings : IHasRegistryParent
    {

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

        internal void Apply(ServiceRegistry services)
        {
            foreach (var setting in _settings)
            {
                setting.Value.Apply(services);
            }
        }
    }
}
