using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Settings;
using StructureMap.TypeRules;

namespace Jasper
{
    public class JasperRegistry : IJasperRegistry
    {
        private readonly Dictionary<Type, IFeature> _features = new Dictionary<Type, IFeature>();

        public JasperRegistry()
        {
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

            Generation = new GenerationConfig($"{ApplicationAssembly.GetName().Name}.Generated");

            Logging = new Logging(this);
            Settings = new JasperSettings(this);
        }

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
                catch (Exception e)
                {
                    // Nothing
                }
            }

            return assembly;
        }

        public Assembly ApplicationAssembly { get; private set; }

        public ServiceRegistry Services { get; } = new ServiceRegistry();

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
}