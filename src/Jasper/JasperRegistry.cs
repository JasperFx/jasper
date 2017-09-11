using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Http;
using Jasper.Http.ContentHandling;
using Jasper.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StructureMap.TypeRules;
using Policies = Jasper.Bus.Configuration.Policies;

namespace Jasper
{
    public class JasperRegistry : IFeatures
    {
        private readonly Dictionary<Type, IFeature> _features = new Dictionary<Type, IFeature>();
        private readonly ServiceRegistry _applicationServices;
        protected readonly ServiceBusFeature _bus;

        public JasperRegistry()
        {
            Features.Include<ConnegDiscoveryFeature>();

            _bus = Features.For<ServiceBusFeature>();

            Http = Features.For<AspNetCoreFeature>();

            Serialization = new SerializationExpression(_bus, this);
            Send = new SendMessageExpression(_bus);

            _applicationServices = new ServiceRegistry();
            ExtensionServices = new ExtensionServiceRegistry();

            Services = _applicationServices;

            ApplicationAssembly = CallingAssembly.DetermineApplicationAssembly(this);

            deriveServiceName();

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            Generation = new GenerationConfig($"{name}.Generated");

            Logging = new Logging(this);
            Settings = new JasperSettings(this);

            Settings.Replace(_bus.Settings);

            if (JasperEnvironment.Name.IsNotEmpty())
            {
                EnvironmentName = JasperEnvironment.Name;
            }
        }

        private void deriveServiceName()
        {
            if (GetType() == typeof(JasperRegistry))
            {
                ServiceName = ApplicationAssembly?.GetName().Name ?? "JasperService";
            }
            else
            {
                ServiceName = GetType().Name.Replace("JasperRegistry", "").Replace("Registry", "");
            }
        }

        public string EnvironmentName
        {
            get => Http.EnvironmentName;
            set => Http.EnvironmentName = value;
        }

        /// <summary>
        /// Options to control how Jasper discovers message handler actions
        /// </summary>
        public HandlerSource Handlers => _bus.Handlers;


        /// <summary>
        /// Register or configure global error handling policies during processing messages
        /// </summary>
        public IHasErrorHandlers ErrorHandling => _bus.Policies;


        public AspNetCoreFeature Http { get; }

        /// <summary>
        /// Configure static message routing rules
        /// </summary>
        public SendMessageExpression Send { get; }

        public ITransportsExpression Transports => _bus.Settings;

        public SerializationExpression Serialization { get; }

        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        public GenerationConfig Generation { get; }

        public Assembly ApplicationAssembly { get; }

        public ServiceRegistry Services { get; private set; }

        public JasperSettings Settings { get; }

        public IFeatures Features => this;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IFeature> GetEnumerator()
        {
            return _features.Values.GetEnumerator();
        }

        void IFeatures.Include<T>()
        {
            if (!_features.ContainsKey(typeof(T)))
            {
                _features.Add(typeof(T), new T());
            }
        }


        T IFeatures.For<T>()
        {
            if (!_features.ContainsKey(typeof(T)))
            {
                _features.Add(typeof(T), new T());
            }

            return _features[typeof(T)].As<T>();
        }

        public Logging Logging { get; }

        internal ServiceRegistry ExtensionServices { get; }


        public string ServiceName
        {
            get => _bus.Settings.ServiceName;
            set => _bus.Settings.ServiceName = value;
        }

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

        public ISubscriptions Subscriptions => _bus.Capabilities;

        public IPublishing Publishing => _bus.Capabilities;


        /// <summary>
        /// Configure rarely used, advanced options
        /// </summary>
        public IAdvancedOptions Advanced => _bus.Settings;
    }

    public interface IFeatures : IEnumerable<IFeature>
    {
        void Include<T>() where T : IFeature, new();
        T For<T>() where T : IFeature, new();
    }


}
