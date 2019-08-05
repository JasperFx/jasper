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

    public class JasperRuntime : IJasperHost
    {
        private readonly IDisposable _host;
        private readonly Lazy<IMessageContext> _bus;
        private IContainer _container;
        private bool _isDisposing;
        private readonly JasperRegistry _registry;



        internal JasperRuntime(IWebHost host)
        {
            _host = host;
            _registry = host.Services.GetRequiredService<JasperRegistry>();
            Container = host.Services.GetService<IContainer>();

            Container.As<Container>().Configure(x => x.AddSingleton(this));

            _bus = new Lazy<IMessageContext>(Get<IMessageContext>);
        }

        private JasperOptions options { get; set; }

        /// <summary>
        ///     The main application assembly for the running application
        /// </summary>
        public Assembly ApplicationAssembly => _registry.ApplicationAssembly;

        /// <summary>
        ///     The underlying Lamar container
        /// </summary>
        public IContainer Container
        {
            get => _container;
            private set
            {
                _container = value;
                options = _container.GetInstance<JasperOptions>();
            }
        }

        public bool IsDisposed { get; private set; }

        public string[] HttpAddresses { get; private set; } = new string[0];

        /// <summary>
        ///     Shortcut to retrieve an instance of the IServiceBus interface for the application
        /// </summary>
        public IMessagePublisher Messaging => _bus.Value;

        /// <summary>
        ///     The logical name of the application from JasperRegistry.ServiceName
        /// </summary>
        public string ServiceName => options?.ServiceName;


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

            _registry.Describe(this, writer);
        }

        /// <summary>
        /// Execute all the environment checks for this application
        /// </summary>
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


        public void Dispose()
        {
            // Because StackOverflowException's are a drag
            if (IsDisposed || _isDisposing) return;

            _isDisposing = true;

            // THis is important to stop every async agent kind of thing
            _container.GetInstance<JasperOptions>().StopAll();

            _host.SafeDispose();

            Container.As<Container>().DisposalLock = DisposalLock.Unlocked;
            Container.Dispose();

            IsDisposed = true;
        }

    }
}
