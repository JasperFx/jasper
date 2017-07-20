using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Bus;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StructureMap;
using StructureMap.TypeRules;

namespace Jasper
{
    public class JasperRegistry : IJasperRegistry
    {
        private readonly Dictionary<Type, IFeature> _features = new Dictionary<Type, IFeature>();
        private readonly ServiceRegistry _applicationServices;
        protected readonly ServiceBusFeature _bus;

        public JasperRegistry()
        {
            _bus = Feature<ServiceBusFeature>();

            Serialization = new JasperBusRegistry.SerializationExpression(_bus);

            _applicationServices = new ServiceRegistry();
            ExtensionServices = new ExtensionServiceRegistry();

            Services = _applicationServices;

            var assembly = this.GetType().GetAssembly();
            var isFeature = assembly.GetCustomAttribute<JasperFeatureAttribute>() != null;
            if (!Equals(assembly, typeof(JasperRegistry).GetAssembly()) && !isFeature)
            {
                ApplicationAssembly = assembly;
            }
            else
            {
                ApplicationAssembly = CallingAssembly.Find();
            }

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            Generation = new GenerationConfig($"{name}.Generated");

            Logging = new Logging(this);
            Settings = new JasperSettings(this);

            // TODO -- this is *temporary*. Will need to at a minimum get this segregated
            // between the web and the service bus
            Settings.Alter<JsonSerializerSettings>(_ => _.TypeNameHandling = TypeNameHandling.All);
        }

        public JasperBusRegistry.SerializationExpression Serialization { get; }

        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        public GenerationConfig Generation { get; }



        public Assembly ApplicationAssembly { get; private set; }

        public ServiceRegistry Services { get; private set; }

        public JasperSettings Settings { get; }

        public void UseFeature<T>() where T : IFeature, new()
        {
            if (!_features.ContainsKey(typeof(T)))
            {
                _features.Add(typeof(T), new T());
            }
        }

        public T Feature<T>() where T : IFeature, new()
        {
            UseFeature<T>();

            return _features[typeof(T)].As<T>();
        }

        public IFeature[] Features => _features.Values.ToArray();

        public Logging Logging { get; }

        internal ServiceRegistry ExtensionServices { get; }

        internal void ApplyExtensions(IJasperExtension[] extensions)
        {
            Settings.ApplyingExtensions = true;
            Services = ExtensionServices;


            foreach (var extension in extensions)
            {
                extension.Configure(this);
            }

            Services = _applicationServices;
            Settings.ApplyingExtensions = false;
        }

        public void Include(IJasperExtension extension)
        {
            ApplyExtensions(new IJasperExtension[]{extension});
        }

        public void Include<T>(Action<T> configure = null) where T : IJasperExtension, new()
        {
            var extension = new T();
            configure?.Invoke(extension);

            Include(extension);
        }

    }

    public class Logging : ILogging
    {
        private readonly JasperRegistry _parent;

        public Logging(JasperRegistry parent)
        {
            _parent = parent;
        }

        JasperRegistry ILogging.Parent => _parent;

        public bool UseConsoleLogging { get; set; } = false;
    }

    public interface ILogging
    {
        JasperRegistry Parent { get; }
    }

    public class ExtensionServiceRegistry : ServiceRegistry{}
}
