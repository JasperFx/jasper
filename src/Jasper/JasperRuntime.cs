using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.EnvironmentChecks;
using Jasper.Http;
using Jasper.Messaging;
using Lamar;
using Lamar.Scanning.Conventions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Jasper
{
    public class JasperRuntime : IDisposable
    {
        private readonly IDisposable _host;
        private readonly Lazy<IMessageContext> _bus;
        private IContainer _container;
        private bool isDisposing;


        private JasperRuntime(JasperRegistry registry, IWebHost host)
        {
            _host = host;
            Registry = registry;
            Container = host.Services.GetService<IContainer>();

            _bus = new Lazy<IMessageContext>(Get<IMessageContext>);
        }

        internal JasperOptions Settings { get; private set; }

        internal JasperRegistry Registry { get; }

        /// <summary>
        ///     The main application assembly for the running application
        /// </summary>
        public Assembly ApplicationAssembly => Registry.ApplicationAssembly;

        /// <summary>
        ///     The underlying Lamar container
        /// </summary>
        public IContainer Container
        {
            get => _container;
            private set
            {
                _container = value;
                Settings = _container.GetInstance<JasperOptions>();
            }
        }

        public bool IsDisposed { get; private set; }

        public string[] HttpAddresses { get; private set; } = new string[0];

        /// <summary>
        ///     Shortcut to retrieve an instance of the IServiceBus interface for the application
        /// </summary>
        public IMessageContext Messaging => _bus.Value;

        /// <summary>
        ///     The logical name of the application from JasperRegistry.ServiceName
        /// </summary>
        public string ServiceName => Settings?.ServiceName;


        /// <summary>
        ///     Creates a Jasper application for the current executing assembly
        ///     using all the default Jasper configurations
        /// </summary>
        /// <returns></returns>
        public static JasperRuntime Basic()
        {
            return BasicAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Creates a Jasper application for the current executing assembly
        ///     using all the default Jasper configurations
        /// </summary>
        /// <returns></returns>
        public static Task<JasperRuntime> BasicAsync()
        {
            return bootstrap(new JasperRegistry());
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static JasperRuntime For(JasperRegistry registry)
        {
            return ForAsync(registry).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static Task<JasperRuntime> ForAsync(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the JasperRegistry of
        ///     type T
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The type of your JasperRegistry</typeparam>
        /// <returns></returns>
        public static JasperRuntime For<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            return ForAsync(configure).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the JasperRegistry of
        ///     type T
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="T">The type of your JasperRegistry</typeparam>
        /// <returns></returns>
        public static Task<JasperRuntime> ForAsync<T>(Action<T> configure = null) where T : JasperRegistry, new()
        {
            var registry = new T();
            configure?.Invoke(registry);

            return bootstrap(registry);
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the configured JasperRegistry
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static JasperRuntime For(Action<JasperRegistry> configure)
        {
            return ForAsync(configure).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the configured JasperRegistry
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Task<JasperRuntime> ForAsync(Action<JasperRegistry> configure)
        {
            var registry = new JasperRegistry();
            configure?.Invoke(registry);

            return bootstrap(registry);
        }


        /// <summary>
        ///     Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return Container.GetInstance<T>();
        }

        /// <summary>
        ///     Shorthand to fetch a service from the application container by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Get(Type type)
        {
            return Container.GetInstance(type);
        }

        /// <summary>
        ///     Writes a textual report about the configured transports and servers
        ///     for this application
        /// </summary>
        /// <param name="writer"></param>
        public void Describe(TextWriter writer)
        {
            writer.WriteLine($"Running service '{ServiceName}'");
            if (ApplicationAssembly != null) writer.WriteLine("Application Assembly: " + ApplicationAssembly.FullName);

            var hosting = Container.TryGetInstance<IHostingEnvironment>();

            if (hosting != null)
            {
                writer.WriteLine($"Hosting environment: {hosting.EnvironmentName}");
                writer.WriteLine($"Content root path: {hosting.ContentRootPath}");
            }

            var hosted = Container.GetAllInstances<IHostedService>();
            foreach (var hostedService in hosted) writer.WriteLine("Hosted Service: " + hostedService);

            Registry.Describe(this, writer);
        }

        public void ExecuteAllEnvironmentChecks()
        {
            var checks = Container.Model.GetAllPossible<IEnvironmentCheck>();

            var recorder = Container.GetInstance<IEnvironmentRecorder>();

            foreach (var check in checks)
                try
                {
                    check.Assert(this);
                    recorder.Success(check.Description);
                }
                catch (Exception e)
                {
                    recorder.Failure(check.Description, e);
                }

            if (Get<JasperOptions>().ThrowOnValidationErrors) recorder.AssertAllSuccessful();
        }

        public static void ApplyExtensions(JasperRegistry registry)
        {
            var assemblies = FindExtensionAssemblies();

            if (!assemblies.Any()) return;

            var extensions = assemblies
                .Select(x => x.GetAttribute<JasperModuleAttribute>().ExtensionType)
                .Where(x => x != null)
                .Select(x => Activator.CreateInstance(x).As<IJasperExtension>())
                .ToArray();

            registry.ApplyExtensions(extensions);
        }

        public static Assembly[] FindExtensionAssemblies()
        {
            return AssemblyFinder
                .FindAssemblies(txt => { }, false)
                .Where(a => a.HasAttribute<JasperModuleAttribute>())
                .ToArray();
        }


        private static Task<JasperRuntime> bootstrap(JasperRegistry registry)
        {
            var host = registry
                .ToWebHostBuilder()
                .Start();

            var runtime = new JasperRuntime(registry, host);

            runtime.Container.As<Container>().Configure(x => x.AddSingleton(runtime));

            return Task.FromResult(runtime);
        }

        public void Dispose()
        {
            // Because StackOverflowException's are a drag
            if (IsDisposed || isDisposing) return;

            isDisposing = true;

            _host.Dispose();

            Container.As<Container>().DisposalLock = DisposalLock.Unlocked;
            Container.Dispose();

            IsDisposed = true;
        }

    }
}
