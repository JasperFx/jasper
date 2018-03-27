using System;
using System.Collections.Generic;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Settings
{
    public interface ISettingsBuilder
    {
        void Apply(WebHostBuilderContext config, JasperRegistry registry);
    }

    public class SettingsBuilder<T> : ISettingsBuilder where T : class
    {
        private Func<WebHostBuilderContext, T> _source;
        private readonly IList<Action<WebHostBuilderContext, T>> _alterations
            = new List<Action<WebHostBuilderContext, T>>();

        private readonly IList<Action<WebHostBuilderContext, T>> _packageAlterations
            = new List<Action<WebHostBuilderContext, T>>();

        private readonly IList<Action<T>> _withs = new List<Action<T>>();



        public SettingsBuilder(Func<WebHostBuilderContext, T> source = null)
        {
            if (source == null)
            {
                _source = r => r.Configuration.Get<T>();
            }
            else
            {
                _source = source;
            }
        }

        internal void SetSource(T settings)
        {
            _source = c => settings;
        }

        public void Replace(Func<WebHostBuilderContext, T> source)
        {
            _alterations.Clear();
            _withs.Clear();

            // clear everything, then:
            _source = source;
        }

        public void Replace(T settings)
        {
            Replace(_ => settings);
        }

        public void PackageAlter(Action<WebHostBuilderContext, T> alteration)
        {
            _packageAlterations.Add(alteration);
        }

        public void Alter(Action<WebHostBuilderContext, T> alteration)
        {
            _alterations.Add(alteration);
        }

        public void With(Action<T> alteration)
        {
            _withs.Add(alteration);
        }

        public void Apply(WebHostBuilderContext config, JasperRegistry registry)
        {
            var settings = _source(config) ?? Activator.CreateInstance(typeof(T)).As<T>();

            foreach (var alteration in _packageAlterations)
            {
                alteration(config, settings);
            }

            foreach (var alteration in _alterations)
            {
                alteration(config, settings);
            }

            foreach (var @using in _withs)
            {
                @using(settings);
            }

            registry.Services.AddSingleton(settings);
        }


    }
}
