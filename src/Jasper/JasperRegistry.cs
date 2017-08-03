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
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Http;
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
            _bus = Features.For<ServiceBusFeature>();
            Http = Features.For<AspNetCoreFeature>();

            Serialization = new SerializationExpression(_bus);
            Channels = new ChannelConfiguration(_bus);
            Messages = new MessagesExpression(_bus);

            _applicationServices = new ServiceRegistry();
            ExtensionServices = new ExtensionServiceRegistry();

            Services = _applicationServices;

            ApplicationAssembly = CallingAssembly.DetermineApplicationAssembly(this);

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            Generation = new GenerationConfig($"{name}.Generated");

            Logging = new Logging(this);
            Settings = new JasperSettings(this);


        }

        public string EnvironmentName
        {
            get => Http.EnvironmentName;
            set => Http.EnvironmentName = value;
        }

        public AspNetCoreFeature Http { get; }

        public MessagesExpression Messages { get; }

        public ChannelConfiguration Channels { get; }

        public SerializationExpression Serialization { get; }

        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        public GenerationConfig Generation { get; }

        public Assembly ApplicationAssembly { get; private set; }

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
            get => _bus.Channels.Name;
            set => _bus.Channels.Name = value;
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

    public interface IFeatures : IEnumerable<IFeature>
    {
        void Include<T>() where T : IFeature, new();
        T For<T>() where T : IFeature, new();
    }
}
