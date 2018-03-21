using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.EnvironmentChecks;
using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Lamar;
using Lamar.Codegen.Variables;
using Lamar.Scanning.Conventions;
using Lamar.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper
{
    public class JasperRuntime : IDisposable
    {
        private readonly Lazy<IMessageContext> _bus;
        private bool isDisposing;

        private JasperRuntime(JasperRegistry registry, IServiceCollection services, PerfTimer timer)
        {
            Bootstrapping = timer;

            services.AddSingleton(this);

            timer.Record("new Container()", () =>
            {
                Container = new Container(services, timer)
                {
                    DisposalLock = DisposalLock.Ignore
                };
            });


            registry.CodeGeneration.Sources.Add(new NowTimeVariableSource());

            registry.CodeGeneration.Assemblies.Add(GetType().GetTypeInfo().Assembly);
            registry.CodeGeneration.Assemblies.Add(registry.ApplicationAssembly);

            Registry = registry;

            _bus = new Lazy<IMessageContext>(Get<IMessageContext>);
        }

        public PerfTimer Bootstrapping { get; }

        internal JasperRegistry Registry { get; }

        /// <summary>
        ///     The main application assembly for the running application
        /// </summary>
        public Assembly ApplicationAssembly => Registry.ApplicationAssembly;

        /// <summary>
        ///     The underlying BlueMilk container
        /// </summary>
        public Container Container { get; private set; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     Summary of all the message handling, subscription, and message publishing
        ///     capabilities of the running Jasper application
        /// </summary>
        public ServiceCapabilities Capabilities { get; internal set; }

        public string[] HttpAddresses => Container.TryGetInstance<IServer>()?.Features?.Get<IServerAddressesFeature>()
                                             ?.Addresses?.ToArray() ??
                                         Registry.HttpAddresses?.Split(';').ToArray() ?? new string[0];

        /// <summary>
        ///     Shortcut to retrieve an instance of the IServiceBus interface for the application
        /// </summary>
        public IMessageContext Messaging => _bus.Value;

        /// <summary>
        ///     The logical name of the application from JasperRegistry.ServiceName
        /// </summary>
        public string ServiceName => Registry.ServiceName;

        /// <summary>
        ///     Information about the running service node as published to service discovery
        /// </summary>
        public IServiceNode Node { get; internal set; }

        public void Dispose()
        {
            // Because StackOverflowException's are a drag
            if (IsDisposed || isDisposing) return;

            try
            {
                Get<INodeDiscovery>().UnregisterLocalNode().Wait(3.Seconds());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to un-register the running node");
                Get<IMessageLogger>().LogException(e);
            }

            Registry.Stop(this).Wait(10.Seconds());

            Get<MessagingSettings>().StopAll();

            isDisposing = true;

            Container.DisposalLock = DisposalLock.Unlocked;
            Container.Dispose();

            IsDisposed = true;
        }

        /// <summary>
        ///     Creates a Jasper application for the current executing assembly
        ///     using all the default Jasper configurations
        /// </summary>
        /// <returns></returns>
        public static JasperRuntime Basic()
        {
            return bootstrap(new JasperRegistry()).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the registry
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static JasperRuntime For(JasperRegistry registry)
        {
            return bootstrap(registry).GetAwaiter().GetResult();
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
            var registry = new T();
            configure?.Invoke(registry);

            return bootstrap(registry).GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Builds and initializes a JasperRuntime for the configured JasperRegistry
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static JasperRuntime For(Action<JasperRegistry> configure)
        {
            var registry = new JasperRegistry();
            configure?.Invoke(registry);

            return bootstrap(registry).GetAwaiter().GetResult();
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


        private static async Task<JasperRuntime> bootstrap(JasperRegistry registry)
        {
            var timer = new PerfTimer();
            timer.Start("Bootstrapping");

            timer.Record("Finding and Applying Extensions", () => { applyExtensions(registry); });

            timer.Record("Bootstrapping Settings", () => registry.Settings.Bootstrap());


            var handlerCompilation = registry.Messaging.CompileHandlers(registry, timer);


            var services = registry.CombinedServices();

            var runtime = new JasperRuntime(registry, services, timer);
            var featureBuilding = registry.BuildFeatures(runtime, timer);

            await handlerCompilation;
            await registry.Messaging.Activate(runtime, registry.CodeGeneration, timer);

            await featureBuilding;

            await registry.Startup(runtime);

            // Run environment checks
            timer.Record("Environment Checks", () =>
            {
                var recorder = EnvironmentChecker.ExecuteAll(runtime);
                if (runtime.Get<MessagingSettings>().ThrowOnValidationErrors) recorder.AssertAllSuccessful();
            });

            timer.MarkStart("Register Node");
            await registerRunningNode(runtime, registry);
            timer.MarkFinished("Register Node");

            timer.Stop();

            return runtime;
        }

        private static async Task registerRunningNode(JasperRuntime runtime, JasperRegistry registry)
        {
            // TODO -- get a helper for this, it's ugly
            var settings = runtime.Get<MessagingSettings>();
            var nodes = runtime.Get<INodeDiscovery>();

            try
            {
                var local = new ServiceNode(settings);
                registry.AlterNode(local);


                local.HttpEndpoints = runtime.HttpAddresses?.Select(x => x.ToUri().ToMachineUri()).Distinct()
                    .ToArray();

                runtime.Node = local;

                await nodes.Register(local);
            }
            catch (Exception e)
            {
                runtime.Get<IMessageLogger>()
                    .LogException(e, message: "Failure when trying to register the node with " + nodes);
            }
        }

        private static void applyExtensions(JasperRegistry registry)
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

            Registry.Describe(this, writer);
        }
    }
}
