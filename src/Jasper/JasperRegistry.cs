using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Baseline;
using BlueMilk.Codegen;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Http;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;

namespace Jasper
{
    public class JasperRegistry : IFeatures
    {
        private readonly ServiceRegistry _applicationServices;
        protected readonly ServiceBusFeature _bus;
        private readonly Dictionary<Type, IFeature> _features = new Dictionary<Type, IFeature>();

        public JasperRegistry()
        {
            Features.Include<ConnegDiscoveryFeature>();

            _bus = Features.For<ServiceBusFeature>();

            Http = Features.For<AspNetCoreFeature>();

            Publish = new PublishingExpression(_bus);

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

        public string EnvironmentName
        {
            get => Http.EnvironmentName;
            set => Http.EnvironmentName = value;
        }

        /// <summary>
        /// Options to control how Jasper discovers message handler actions, error
        /// handling and other policies on message handling
        /// </summary>
        public HandlerSource Handlers => _bus.Handlers;

        public AspNetCoreFeature Http { get; }

        /// <summary>
        /// Configure static message routing rules and message publishing rules
        /// </summary>
        public PublishingExpression Publish { get; }

        /// <summary>
        /// Configure or disable the built in transports
        /// </summary>
        public ITransportsExpression Transports => _bus.Settings;

        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        // TODO -- move this to advanced too? Won't be used very often
        public GenerationConfig Generation { get; }

        // TODO -- does this need to be public?
        public Assembly ApplicationAssembly { get; }

        public ServiceRegistry Services { get; private set; }

        public JasperSettings Settings { get; }

        // TODO -- move this to advanced
        public IFeatures Features => this;

        // TODO -- move this to advanced
        public Logging Logging { get; }

        internal ServiceRegistry ExtensionServices { get; }


        public string ServiceName
        {
            get => _bus.Settings.ServiceName;
            set => _bus.Settings.ServiceName = value;
        }


        public ISubscriptions Subscribe => _bus.Capabilities;

        /// <summary>
        /// Configure uncommonly used, advanced options
        /// </summary>
        public IAdvancedOptions Advanced => _bus.Settings;

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
                _features.Add(typeof(T), new T());
        }


        T IFeatures.For<T>()
        {
            if (!_features.ContainsKey(typeof(T)))
                _features.Add(typeof(T), new T());

            return _features[typeof(T)].As<T>();
        }

        private void deriveServiceName()
        {
            if (GetType() == typeof(JasperRegistry))
                ServiceName = ApplicationAssembly?.GetName().Name ?? "JasperService";
            else
                ServiceName = GetType().Name.Replace("JasperRegistry", "").Replace("Registry", "");
        }

        internal void ApplyExtensions(IJasperExtension[] extensions)
        {
            Settings.ApplyingExtensions = true;
            Services = ExtensionServices;


            foreach (var extension in extensions)
                extension.Configure(this);

            Services = _applicationServices;
            Settings.ApplyingExtensions = false;
        }

        public void Include(IJasperExtension extension)
        {
            ApplyExtensions(new[] {extension});
        }

        public void Include<T>(Action<T> configure = null) where T : IJasperExtension, new()
        {
            var extension = new T();
            configure?.Invoke(extension);

            Include(extension);
        }
    }

    public interface IFeatures : IEnumerable<IFeature>
    {
        void Include<T>() where T : IFeature, new();
        T For<T>() where T : IFeature, new();
    }
}
