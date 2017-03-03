using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Configuration;
using StructureMap.TypeRules;

namespace Jasper
{
    public interface IJasperRegistry
    {
        T Feature<T>() where T : IFeature, new();
        IFeature[] Features { get; }
        Assembly ApplicationAssembly { get; }
    }

    public class JasperRegistry : IJasperRegistry
    {
        private readonly Dictionary<Type, IFeature> _features = new Dictionary<Type, IFeature>();


        public JasperRegistry()
        {
            var assembly = this.GetType().GetAssembly();
            if (assembly != typeof(JasperRegistry).GetAssembly())
            {
                ApplicationAssembly = assembly;
            }
        }

        public Assembly ApplicationAssembly { get; private set; }

        public ServiceRegistry Services { get; } = new ServiceRegistry();

        /// <summary>
        /// Convenience method to set the application assembly by using a Type
        /// contained in that Assembly
        /// </summary>
        /// <typeparam name="T">A type contained within the application assembly</typeparam>
        public void ApplicationContains<T>()
        {
            ApplicationAssembly = typeof(T).GetAssembly();
        }

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
    }
}