using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Baseline;
using BlueMilk;
using BlueMilk.Codegen;
using BlueMilk.Scanning;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Jasper.Configuration;
using Jasper.Conneg;
using Jasper.Http;
using Jasper.Settings;
using Jasper.Util;
using Microsoft.Extensions.Configuration;

namespace Jasper
{


    /// <summary>
    /// Completely defines and configures a Jasper application
    /// </summary>
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
            Generation = new GenerationRules($"{name}.Generated");

            Logging = new Logging(this);
            Settings = new JasperSettings(this);

            Settings.Replace(_bus.Settings);
            Settings.Replace(Http.Settings);

            if (JasperEnvironment.Name.IsNotEmpty())
            {
                EnvironmentName = JasperEnvironment.Name;
            }

            EnvironmentChecks = new EnvironmentCheckExpression(this);
        }

        internal BusSettings BusSettings => _bus.Settings;

        /// <summary>
        /// Configure worker queue priority, message assignement, and worker
        /// durability
        /// </summary>
        public IWorkersExpression Processing => _bus.Settings.Workers;

        /// <summary>
        /// Register environment checks to debug application bootstrapping failures
        /// </summary>
        public EnvironmentCheckExpression EnvironmentChecks { get; }

        /// <summary>
        /// Gets or sets the ASP.Net Core environment names
        /// </summary>
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

        /// <summary>
        /// IWebHostBuilder and other configuration for ASP.net Core usage within a Jasper
        /// application
        /// </summary>
        public AspNetCoreFeature Http { get; }

        /// <summary>
        /// Configure static message routing rules and message publishing rules
        /// </summary>
        public PublishingExpression Publish { get; }

        /// <summary>
        /// Configure or disable the built in transports
        /// </summary>
        public ITransportsExpression Transports => _bus.Settings;

        /// <summary>
        /// Use to load and apply configuration sources within the application
        /// </summary>
        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        // TODO -- move this to advanced too? Won't be used very often
        public GenerationRules Generation { get; }

        public Assembly ApplicationAssembly { get; }

        /// <summary>
        /// Register additional services to the underlying IoC container
        /// </summary>
        public ServiceRegistry Services { get; private set; }

        /// <summary>
        /// Access to the strong typed configuration settings and alterations within
        /// a Jasper application
        /// </summary>
        public JasperSettings Settings { get; }

        // TODO -- move this to advanced
        public IFeatures Features => this;

        /// <summary>
        /// Use to configure or customize Jasper event logging
        /// </summary>
        public Logging Logging { get; }

        internal ServiceRegistry ExtensionServices { get; }

        /// <summary>
        /// Gets or sets the logical service name for this Jasper application. By default,
        /// this is derived from the name of the JasperRegistry class
        /// </summary>
        public string ServiceName
        {
            get => _bus.Settings.ServiceName;
            set => _bus.Settings.ServiceName = value;
        }

        /// <summary>
        /// Configure dynamic subscriptions to this application
        /// </summary>
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

        /// <summary>
        /// Applies the extension to this application
        /// </summary>
        /// <param name="extension"></param>
        public void Include(IJasperExtension extension)
        {
            ApplyExtensions(new[] {extension});
        }

        /// <summary>
        /// Applies the extension with optional configuration to the application
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T"></typeparam>
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
