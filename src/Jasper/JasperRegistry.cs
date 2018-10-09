using System;
using System.IO;
using System.Reflection;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Settings;
using Jasper.Util;
using Lamar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Jasper
{
    /// <summary>
    ///     Completely defines and configures a Jasper application
    /// </summary>
    public partial class JasperRegistry
    {
        private static Assembly _rememberedCallingAssembly;

        private readonly ServiceRegistry _applicationServices = new ServiceRegistry();
        protected readonly ServiceRegistry _baseServices;

        static JasperRegistry()
        {
            Container.Warmup();
        }

        public JasperRegistry()
        {
            Configuration.SetBasePath(Directory.GetCurrentDirectory());

            Publish = new PublishingExpression(Messaging);

            HttpRoutes = new HttpSettings(Messaging.Settings);

            Services = _applicationServices;

            establishApplicationAssembly();


            deriveServiceName();

            var name = ApplicationAssembly?.GetName().Name ?? "JasperApplication";
            CodeGeneration = new JasperGenerationRules($"{name.Replace(".", "_")}_Generated");

            _baseServices = new JasperServiceRegistry(this);

            Settings = new JasperSettings(this);

            Settings.Replace(Messaging.Settings);


            Hosting = this;

            // ASP.Net Core will freak out if this isn't there
            EnvironmentConfiguration[WebHostDefaults.ApplicationKey] = ApplicationAssembly.FullName;

            Settings.Replace(HttpRoutes);
        }

        /// <summary>
        ///     Use to load and apply configuration sources within the application
        /// </summary>
        public ConfigurationBuilder Configuration { get; } = new ConfigurationBuilder();


        /// <summary>
        ///     Configure how HTTP routes are discovered and handled
        /// </summary>
        public HttpSettings HttpRoutes { get; }

        /// <summary>
        ///     Configure ASP.Net Core hosting for this Jasper application
        /// </summary>
        public IWebHostBuilder Hosting { get; }


        /// <summary>
        ///     Configure or extend the Lamar code generation
        /// </summary>
        public JasperGenerationRules CodeGeneration { get; }

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


        protected internal void Describe(JasperRuntime runtime, TextWriter writer)
        {
            Messaging.Describe(runtime, writer);
            HttpRoutes.Describe(runtime, writer);
        }
    }
}
