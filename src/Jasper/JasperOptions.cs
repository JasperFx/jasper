using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Baseline;
using BaselineTypeDiscovery;
using Jasper.Configuration;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Settings;
using Jasper.Util;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Jasper
{
    /// <summary>
    ///     Completely defines and configures a Jasper application
    /// </summary>
    public partial class JasperOptions
    {
        protected static Assembly _rememberedCallingAssembly;

        protected readonly ServiceRegistry _applicationServices = new ServiceRegistry();

        private readonly List<IJasperExtension> _appliedExtensions = new List<IJasperExtension>();
        protected readonly ServiceRegistry _baseServices;

        private readonly IList<Type> _extensionTypes = new List<Type>();


        public JasperOptions() : this(null)
        {
        }

        public JasperOptions(string assemblyName)
        {
            Services = _applicationServices;

            establishApplicationAssembly(assemblyName);

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            CodeGeneration = new GenerationRules($"{name.Replace(".", "_")}_Generated");
            CodeGeneration.Sources.Add(new NowTimeVariableSource());

            CodeGeneration.Assemblies.Add(GetType().GetTypeInfo().Assembly);
            CodeGeneration.Assemblies.Add(ApplicationAssembly);


            _baseServices = new JasperServiceRegistry(this);

            _baseServices.AddSingleton(CodeGeneration);

            Settings = new SettingsGraph(this);

            deriveServiceName();
        }



        /// <summary>
        /// Advanced configuration options for Jasper message processing,
        /// job scheduling, validation, and resiliency features
        /// </summary>
        public AdvancedSettings Advanced { get; } = new AdvancedSettings();


        /// <summary>
        ///     Configure or extend the Lamar code generation
        /// </summary>
        public GenerationRules CodeGeneration { get; }

        /// <summary>
        ///     Register additional services to the underlying IoC container
        /// </summary>
        public ServiceRegistry Services { get; private set; }

        /// <summary>
        ///     Access to the strong typed configuration settings and alterations within
        ///     a Jasper application
        /// </summary>
        public SettingsGraph Settings { get; }

        /// <summary>
        ///     The main application assembly for this Jasper system
        /// </summary>
        public Assembly ApplicationAssembly { get; private set; }

        internal HandlerGraph HandlerGraph { get; } = new HandlerGraph();

        /// <summary>
        ///     Options to control how Jasper discovers message handler actions, error
        ///     handling, local worker queues, and other policies on message handling
        /// </summary>
        public IHandlerConfiguration Handlers => HandlerGraph;


        /// <summary>
        /// Configure message listeners or sending endpoints
        /// </summary>
        public IEndpoints Endpoints => Transports;

        internal TransportCollection Transports { get; } = new TransportCollection();

        internal ServiceRegistry ExtensionServices { get; } = new ExtensionServiceRegistry();

        /// <summary>
        ///     Read only view of the extensions that have been applied to this
        ///     JasperOptions
        /// </summary>
        public IReadOnlyList<IJasperExtension> AppliedExtensions => _appliedExtensions;

        private void deriveServiceName()
        {
            if (GetType() == typeof(JasperOptions))
                Advanced.ServiceName = ApplicationAssembly?.GetName().Name ?? "JasperService";
            else
                Advanced.ServiceName  = GetType().Name.Replace("JasperOptions", "").Replace("Registry", "").Replace("Options", "");
        }

        /// <summary>
        ///     Get or set the logical Jasper service name. By default, this is
        ///     derived from the name of a custom JasperOptions
        /// </summary>
        public string ServiceName
        {
            get => Advanced.ServiceName;
            set => Advanced.ServiceName = value;
        }

        /// <summary>
        /// Direct access to configure or add local queues for durability,
        /// maximum parallel threads, and other settings
        /// </summary>
        [Obsolete("pull this into Endpoints")]
        public ILocalQueues LocalQueues => Transports.Get<LocalTransport>();

        private void establishApplicationAssembly(string assemblyName)
        {
            if (assemblyName.IsNotEmpty())
            {
                ApplicationAssembly = Assembly.Load(assemblyName);
            }
            else if (GetType() == typeof(JasperOptions) || GetType() == typeof(JasperOptions))
            {
                if (_rememberedCallingAssembly == null)
                    _rememberedCallingAssembly = CallingAssembly.DetermineApplicationAssembly(this);

                ApplicationAssembly = _rememberedCallingAssembly;
            }
            else
            {
                ApplicationAssembly = CallingAssembly.DetermineApplicationAssembly(this);
            }

            if (ApplicationAssembly == null)
                throw new InvalidOperationException("Unable to determine an application assembly");
        }

        internal void ApplyExtensions(IJasperExtension[] extensions)
        {
            // Apply idempotency
            extensions = extensions.Where(x => !_extensionTypes.Contains(x.GetType())).ToArray();

            Settings.ApplyingExtensions = true;
            Services = ExtensionServices;


            foreach (var extension in extensions)
            {
                extension.Configure(this);
                _appliedExtensions.Add(extension);
            }


            Services = _applicationServices;
            Settings.ApplyingExtensions = false;

            _extensionTypes.Fill(extensions.Select(x => x.GetType()));
        }

        /// <summary>
        ///     Applies the extension to this application
        /// </summary>
        /// <param name="extension"></param>
        public void Include(IJasperExtension extension)
        {
            ApplyExtensions(new[] {extension});
        }

        /// <summary>
        ///     Applies the extension with optional configuration to the application
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T"></typeparam>
        public void Include<T>(Action<T> configure = null) where T : IJasperExtension, new()
        {
            var extension = new T();
            configure?.Invoke(extension);

            Include(extension);
        }

        internal ServiceRegistry CombineServices()
        {
            var all = _baseServices.Concat(ExtensionServices).Concat(_applicationServices);

            var combined = new ServiceRegistry();
            combined.AddRange(all);

            return combined;
        }

        /// <summary>
        /// Can be overridden to perform any kind of hosting environment or configuration dependent
        /// configuration of your Jasper application
        /// </summary>
        /// <param name="hosting"></param>
        /// <param name="config"></param>
        public virtual void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Nothing
        }



    }

}
