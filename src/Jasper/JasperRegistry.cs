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

        public JasperRegistry()
        {
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
                ApplicationAssembly = findTheCallingAssembly();
            }

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            Generation = new GenerationConfig($"{name}.Generated");

            Logging = new Logging(this);
            Settings = new JasperSettings(this);

            // TODO -- this is *temporary*. Will need to at a minimum get this segregated
            // between the web and the service bus
            Settings.Alter<JsonSerializerSettings>(_ => _.TypeNameHandling = TypeNameHandling.All);
        }

        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        public GenerationConfig Generation { get; }

        private static Assembly findTheCallingAssembly()
        {
            string trace = Environment.StackTrace;



            var parts = trace.Split('\n');

            for (int i = 4; i < parts.Length; i++)
            {
                var line = parts[i];
                var assembly = findAssembly(line);
                if (assembly != null && !isSystemAssembly(assembly))
                {
                    return assembly;
                }
            }

            return null;
        }

        private static bool isSystemAssembly(Assembly assembly)
        {
            if (assembly == null) return false;

            if (assembly.GetCustomAttributes<JasperFeatureAttribute>().Any()) return true;

            if (assembly.GetName().Name == "Jasper") return true;

            return assembly.GetName().Name.StartsWith("System.");
        }

        private static Assembly findAssembly(string stacktraceLine)
        {
            var candidate = stacktraceLine.Trim().Substring(3);

            // Short circuit this
            if (candidate.StartsWith("System.")) return null;

            Assembly assembly = null;
            var names = candidate.Split('.');
            for (var i = names.Length - 2; i > 0; i--)
            {
                var possibility = string.Join(".", names.Take(i).ToArray());

                try
                {

                    assembly = System.Reflection.Assembly.Load(new AssemblyName(possibility));
                    break;
                }
                catch
                {
                    // Nothing
                }
            }

            return assembly;
        }

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
