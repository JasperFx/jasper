using System;
using System.Collections.Generic;
using Baseline;
using Lamar;
using Microsoft.Extensions.Hosting;

namespace Jasper.Settings
{
    public interface IHasRegistryParent
    {
        JasperOptions Parent { get; }
    }

    public class SettingsGraph : IHasRegistryParent
    {
        private readonly JasperOptions _parent;

        private readonly Dictionary<Type, ISettingsBuilder> _settings
            = new Dictionary<Type, ISettingsBuilder>();

        public SettingsGraph(JasperOptions parent)
        {
            _parent = parent;
        }

        internal bool ApplyingExtensions { private get; set; }

        JasperOptions IHasRegistryParent.Parent => _parent;

        private SettingsBuilder<T> forType<T>(Func<HostBuilderContext, T> source = null) where T : class
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
        public void Alter<T>(Action<HostBuilderContext, T> alteration) where T : class, new()
        {
            var builder = forType<T>();
            if (ApplyingExtensions)
                builder.PackageAlter(alteration);
            else
                builder.Alter(alteration);
        }

        internal void Apply(ServiceRegistry services)
        {
            foreach (var setting in _settings)
            {
                setting.Value.Apply(services);
            }
        }

        public void Require<T>() where T : class
        {
            forType<T>();
        }

        public void Replace<T>(T options) where T : class
        {
            forType<T>().Replace(options);
        }
    }
}
