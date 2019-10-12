using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Configuration;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.Settings
{
    public class SettingsBuilder<T> : ISettingsBuilder where T : class
    {
        private readonly IList<Action<HostBuilderContext, T>> _alterations
            = new List<Action<HostBuilderContext, T>>();

        private readonly IList<Action<HostBuilderContext, T>> _packageAlterations
            = new List<Action<HostBuilderContext, T>>();

        private readonly IList<Action<T>> _withs = new List<Action<T>>();
        private Func<HostBuilderContext, T> _source;


        public SettingsBuilder(Func<HostBuilderContext, T> source = null)
        {
            if (source == null)
                _source = r =>
                {
                    var sectionName = typeof(T).ConfigSectionName();
                    return r.Configuration.GetSection(sectionName).Get<T>();
                };
            else
                _source = source;
        }

        private T Build(HostBuilderContext context)
        {
            var settings = _source(context) ?? Activator.CreateInstance(typeof(T)).As<T>();

            foreach (var alteration in _packageAlterations) alteration(context, settings);

            foreach (var alteration in _alterations) alteration(context, settings);

            foreach (var @using in _withs) @using(settings);
            return settings;
        }

        public void Apply(ServiceRegistry services)
        {
            services.For<T>().Use(Build).Singleton();
        }

        public T Build(IServiceContext c)
        {
            var context = new HostBuilderContext(new Dictionary<object, object>())
            {
                Configuration = c.GetInstance<IConfiguration>(),
                HostingEnvironment = c.GetInstance<IHostEnvironment>()
            };

            return Build(context);
        }

        public void Replace(Func<HostBuilderContext, T> source)
        {
            _source = source;
        }

        public void Replace(T settings)
        {
            Value = settings;
            Replace(_ => settings);
        }

        public object Value { get; private set; }

        public void PackageAlter(Action<HostBuilderContext, T> alteration)
        {
            _packageAlterations.Add(alteration);
        }

        public void Alter(Action<HostBuilderContext, T> alteration)
        {
            _alterations.Add(alteration);
        }

    }
}
