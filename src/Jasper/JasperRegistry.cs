using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Settings;
using Jasper.Util;
using Lamar;
using Lamar.Codegen;
using Lamar.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper
{
    /// <summary>
    ///     Completely defines and configures a Jasper application
    /// </summary>
    public partial class JasperRegistry
    {
        static JasperRegistry()
        {
            Container.Warmup();
        }

        private static Assembly _rememberedCallingAssembly;

        private readonly ServiceRegistry _applicationServices = new ServiceRegistry();
        protected readonly ServiceRegistry _baseServices;

        public JasperRegistry()
        {
            Publish = new PublishingExpression(Messaging);

            Services = _applicationServices;

            establishApplicationAssembly();

            _baseServices = new JasperServiceRegistry(this);


            deriveServiceName();

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            CodeGeneration = new GenerationRules($"{name.Replace(".", "_")}_Generated");


            Settings = new JasperSettings(this);

            Settings.Replace(Messaging.Settings);


            Hosting = this;

            // ASP.Net Core will freak out if this isn't there
            EnvironmentConfiguration[WebHostDefaults.ApplicationKey] = ApplicationAssembly.FullName;

            Settings.Replace(Http.Settings);
            Settings.Replace(Http.Transport.As<HttpTransportSettings>()); // Hokey, but I'll allow it

        }

        /// <summary>
        ///     IWebHostBuilder and other configuration for ASP.net Core usage within a Jasper
        ///     application
        /// </summary>
        public AspNetCoreFeature Http { get; } = new AspNetCoreFeature();

        /// <summary>
        /// Configure ASP.Net Core Hosting
        /// </summary>
        public IWebHostBuilder Hosting { get; }


        /// <summary>
        ///     Use to load and apply configuration sources within the application
        /// </summary>
        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();

        /// <summary>
        ///     Configure or extend the BlueMilk code generation
        /// </summary>
        public GenerationRules CodeGeneration { get; }

        /// <summary>
        ///     The main application assembly for this Jasper system
        /// </summary>
        public Assembly ApplicationAssembly { get; private set; }

        /// <summary>
        ///     Register additional services to the underlying IoC container
        /// </summary>
        public ServiceRegistry Services { get; private set; }

        /// <summary>
        ///     Access to the strong typed configuration settings and alterations within
        ///     a Jasper application
        /// </summary>
        public JasperSettings Settings { get; }




        internal string HttpAddresses => EnvironmentConfiguration[WebHostDefaults.ServerUrlsKey];

        private void establishApplicationAssembly()
        {
            if (GetType() == typeof(JasperRegistry))
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

        private void deriveServiceName()
        {
            if (GetType() == typeof(JasperRegistry))
                ServiceName = ApplicationAssembly?.GetName().Name ?? "JasperService";
            else
                ServiceName = GetType().Name.Replace("JasperRegistry", "").Replace("Registry", "");
        }



        internal ServiceRegistry CombinedServices()
        {
            _baseServices.Insert(0, ServiceDescriptor.Singleton<IHostedService, MessagingActivator>());

            RegisterAspNetCoreServices();
            var all = _baseServices.Concat(ExtensionServices).Concat(_applicationServices);

            var combined = new ServiceRegistry();
            combined.AddRange(all);

            combined.For<IPersistence>().UseIfNone<NulloPersistence>();

            return combined;
        }

        protected internal virtual Task BuildFeatures(JasperRuntime runtime, PerfTimer timer)
        {
            return Task.CompletedTask;
        }

        protected internal virtual void Describe(JasperRuntime runtime, TextWriter writer)
        {
            Messaging.Describe(runtime, writer);
        }

    }
}
