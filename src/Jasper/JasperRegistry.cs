using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.ErrorHandling;
using Jasper.Bus.Runtime;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StructureMap.TypeRules;
using Policies = Jasper.Bus.Configuration.Policies;

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

            Serialization = new SerializationExpression(_bus);
            Channels = new ChannelConfiguration(_bus);
            Messages = new MessagesExpression(_bus);

            _applicationServices = new ServiceRegistry();
            ExtensionServices = new ExtensionServiceRegistry();

            Services = _applicationServices;

            determineApplicationAssembly();

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            Generation = new GenerationConfig($"{name}.Generated");

            Logging = new Logging(this);
            Settings = new JasperSettings(this);

        }

        private void determineApplicationAssembly()
        {
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
        }

        public IWebHostBuilder AspNetCore => Settings;

        public DelayedJobExpression DelayedJobs => new DelayedJobExpression(_bus);

        public MessagesExpression Messages { get; }

        public ChannelConfiguration Channels { get; }

        public SerializationExpression Serialization { get; }

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
        public HandlerSource Handlers => _bus.Handlers;
        public Policies Policies => _bus.Policies;

        public string ServiceName
        {
            get => _bus.Channels.Name;
            set => _bus.Channels.Name = value;
        }

        public IHasErrorHandlers ErrorHandling => Policies;

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

        public SubscriptionExpression SubscribeAt(string receiving)
        {
            return SubscribeAt(receiving.ToUri());
        }

        public SubscriptionExpression SubscribeAt(Uri receiving)
        {
            return new SubscriptionExpression(_bus, receiving);
        }

        public SubscriptionExpression SubscribeLocally()
        {
            return new SubscriptionExpression(_bus, null);
        }
    }
}
