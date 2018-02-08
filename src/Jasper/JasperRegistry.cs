using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.Settings;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using CallingAssembly = Jasper.Util.CallingAssembly;

namespace Jasper
{


    /// <summary>
    /// Completely defines and configures a Jasper application
    /// </summary>
    public class JasperRegistry
    {
        private static Assembly _rememberedCallingAssembly;

        private readonly ServiceRegistry _applicationServices = new ServiceRegistry();
        private readonly JasperServiceRegistry _baseServices;


        public JasperRegistry()
        {
            Logging = new Logging(this);

            Publish = new PublishingExpression(Bus);

            ExtensionServices = new ExtensionServiceRegistry();

            Services = _applicationServices;

            establishApplicationAssembly();

            _baseServices = new JasperServiceRegistry(this);


            deriveServiceName();

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            Generation = new GenerationRules($"{name}.Generated");


            Settings = new JasperSettings(this);

            Settings.Replace(Bus.Settings);
            Settings.Replace(Http.Settings);

            if (JasperEnvironment.Name.IsNotEmpty())
            {
                EnvironmentName = JasperEnvironment.Name;
            }

            EnvironmentChecks = new EnvironmentCheckExpression(this);
        }

        private void establishApplicationAssembly()
        {
            if (GetType() == typeof(JasperRegistry))
            {
                if (_rememberedCallingAssembly == null)
                {
                    _rememberedCallingAssembly = CallingAssembly.DetermineApplicationAssembly(this);
                }

                ApplicationAssembly = _rememberedCallingAssembly;
            }
            else
            {
                ApplicationAssembly = CallingAssembly.DetermineApplicationAssembly(this);
            }

            if (ApplicationAssembly == null) throw new InvalidOperationException("Unable to determine an application assembly");
        }


        internal ServiceBusFeature Bus { get; } = new ServiceBusFeature();
        internal BusSettings BusSettings => Bus.Settings;

        /// <summary>
        /// Configure worker queue priority, message assignement, and worker
        /// durability
        /// </summary>
        public IWorkersExpression Processing => Bus.Settings.Workers;

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
        public HandlerSource Handlers => Bus.Handlers;

        /// <summary>
        /// IWebHostBuilder and other configuration for ASP.net Core usage within a Jasper
        /// application
        /// </summary>
        public AspNetCoreFeature Http { get; } = new AspNetCoreFeature();

        /// <summary>
        /// Configure static message routing rules and message publishing rules
        /// </summary>
        public PublishingExpression Publish { get; }

        /// <summary>
        /// Configure or disable the built in transports
        /// </summary>
        public ITransportsExpression Transports => Bus.Settings;

        /// <summary>
        /// Use to load and apply configuration sources within the application
        /// </summary>
        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        // TODO -- move this to advanced too? Won't be used very often
        public GenerationRules Generation { get; }

        public Assembly ApplicationAssembly { get; set; }

        /// <summary>
        /// Register additional services to the underlying IoC container
        /// </summary>
        public ServiceRegistry Services { get; private set; }

        /// <summary>
        /// Access to the strong typed configuration settings and alterations within
        /// a Jasper application
        /// </summary>
        public JasperSettings Settings { get; }

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
            get => Bus.Settings.ServiceName;
            set => Bus.Settings.ServiceName = value;
        }

        /// <summary>
        /// Configure dynamic subscriptions to this application
        /// </summary>
        public ISubscriptions Subscribe => Bus.Capabilities;

        /// <summary>
        /// Configure uncommonly used, advanced options
        /// </summary>
        public IAdvancedOptions Advanced => Bus.Settings;

        public virtual string HttpAddresses => Http.As<IWebHostBuilder>().GetSetting(WebHostDefaults.ServerUrlsKey);

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


        internal ServiceRegistry CombinedServices()
        {
            var all = _baseServices.Concat(ExtensionServices).Concat(_applicationServices);

            var combined = new ServiceRegistry();
            combined.AddRange(all);

            return combined;
        }
    }
}
