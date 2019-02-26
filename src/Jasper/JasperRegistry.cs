using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Http.Routing;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Transports;
using Jasper.Settings;
using Jasper.Util;
using Lamar;
using Lamar.Scanning.Conventions;
using LamarCompiler;
using LamarCompiler.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper
{
    /// <summary>
    ///     Completely defines and configures a Jasper application
    /// </summary>
    public class JasperRegistry : IFullTransportsExpression
    {
        static JasperRegistry()
        {
            Container.Warmup();
        }

        private readonly IList<Action<IWebHostBuilder>> _builderAlterations
            = new List<Action<IWebHostBuilder>>();

        protected static Assembly _rememberedCallingAssembly;

        protected readonly ServiceRegistry _applicationServices = new ServiceRegistry();
        protected readonly ServiceRegistry _baseServices;

        public JasperRegistry() : this(null)
        {
        }

        public JasperRegistry(string assemblyName)
        {
            HttpRoutes = new HttpSettings();

            Services = _applicationServices;

            establishApplicationAssembly(assemblyName);

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            CodeGeneration = new JasperGenerationRules($"{name.Replace(".", "_")}_Generated");
            CodeGeneration.Sources.Add(new NowTimeVariableSource());

            CodeGeneration.Assemblies.Add(GetType().GetTypeInfo().Assembly);
            CodeGeneration.Assemblies.Add(ApplicationAssembly);


            _baseServices = new JasperServiceRegistry(this);

            _baseServices.AddSingleton<GenerationRules>(CodeGeneration);

            Settings = new JasperSettings(this);
            Settings.BindToConfigSection<JasperOptions>("Jasper");



            Publish = new PublishingExpression(Settings, Messaging);
            Settings.Replace(HttpRoutes);

            deriveServiceName();

        }

        /// <summary>
        /// Apply configuration and alterations directly to the underlying
        /// IWebHostBuilder of the running application when this JasperRegistry
        /// is executed
        /// </summary>
        /// <param name="configure"></param>
        public void Hosting(Action<IWebHostBuilder> configure)
        {
            _builderAlterations.Add(configure);
        }

        internal void ConfigureWebHostBuilder(IWebHostBuilder builder)
        {
            foreach (var alteration in _builderAlterations)
            {
                alteration(builder);
            }
        }



        private void deriveServiceName()
        {
            if (GetType() == typeof(JasperRegistry))
                ServiceName = ApplicationAssembly?.GetName().Name ?? "JasperService";
            else
                ServiceName = GetType().Name.Replace("JasperRegistry", "").Replace("Registry", "");
        }

        /// <summary>
        ///     Configure how HTTP routes are discovered and handled
        /// </summary>
        public HttpSettings HttpRoutes { get; }

        /// <summary>
        ///     Configure or extend the Lamar code generation
        /// </summary>
        public JasperGenerationRules CodeGeneration { get; }

        /// <summary>
        ///     Register additional services to the underlying IoC container
        /// </summary>
        public ServiceRegistry Services { get; private set; }

        /// <summary>
        ///     Access to the strong typed configuration settings and alterations within
        ///     a Jasper application
        /// </summary>
        public JasperSettings Settings { get; }

        /// <summary>
        ///     The main application assembly for this Jasper system
        /// </summary>
        public Assembly ApplicationAssembly { get; private set; }

        private void establishApplicationAssembly(string assemblyName)
        {
            if (assemblyName.IsNotEmpty())
            {
                ApplicationAssembly = Assembly.Load(assemblyName);
            }
            else if ((GetType() == typeof(JasperRegistry) || GetType() == typeof(JasperRegistry)))
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

            if (ApplicationAssembly == null)
                throw new InvalidOperationException("Unable to determine an application assembly");
        }

        protected internal void Describe(IJasperHost runtime, TextWriter writer)
        {
            Messaging.Describe(runtime, writer);
            HttpRoutes.Describe(runtime, writer);
        }

        private string _serviceName;
        internal MessagingConfiguration Messaging { get; } = new MessagingConfiguration();

        /// <summary>
        ///     Options to control how Jasper discovers message handler actions, error
        ///     handling, local worker queues, and other policies on message handling
        /// </summary>
        public IHandlerConfiguration Handlers => Messaging.Handling;


        /// <summary>
        ///     Configure static message routing rules and message publishing rules
        /// </summary>
        public PublishingExpression Publish { get; }

        /// <summary>
        ///     Configure or disable the built in transports
        /// </summary>
        public IFullTransportsExpression Transports => this;

        /// <summary>
        ///     Get or set the logical Jasper service name. By default, this is
        ///     derived from the name of a custom JasperRegistry
        /// </summary>
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                _serviceName = value;
                Settings.Messaging(x => x.ServiceName = value);
            }
        }


        void ITransportsExpression.ListenForMessagesFrom(Uri uri)
        {
            Settings.Alter<JasperOptions>(x => x.ListenForMessagesFrom(uri));
        }

        void ITransportsExpression.ListenForMessagesFrom(string uriString)
        {
            Settings.Alter<JasperOptions>(x => x.ListenForMessagesFrom(uriString.ToUri()));
        }

        void ITransportsExpression.EnableTransport(string protocol)
        {
            Settings.Alter<JasperOptions>(x => x.EnableTransport(protocol));
        }

        void ITransportsExpression.DisableTransport(string protocol)
        {
            Settings.Alter<JasperOptions>(x => x.DisableTransport(protocol));
        }

        void IFullTransportsExpression.ListenForMessagesFromUriValueInConfig(string configKey)
        {
            Settings.Messaging((c, options) => options.ListenForMessagesFrom(c.Configuration.TryGetUri(configKey)));
        }

        private readonly List<IJasperExtension> _appliedExtensions = new List<IJasperExtension>();

        internal ServiceRegistry ExtensionServices { get; } = new ExtensionServiceRegistry();

        /// <summary>
        ///     Read only view of the extensions that have been applied to this
        ///     JasperRegistry
        /// </summary>
        public IReadOnlyList<IJasperExtension> AppliedExtensions => _appliedExtensions;

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

        private readonly IList<Type> _extensionTypes = new List<Type>();

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

        public ServiceRegistry CombineServices()
        {
            var all = _baseServices.Concat(ExtensionServices).Concat(_applicationServices);

            var combined = new ServiceRegistry();
            combined.AddRange(all);

            combined.For<IDurableMessagingFactory>().UseIfNone<NulloDurableMessagingFactory>();

            return combined;
        }
    }
}
