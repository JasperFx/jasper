using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Messaging.Bootstrapping;
using Jasper.Testing.Messaging.Compilation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Jasper.Testing.Bootstrapping
{
    // make collection
    // test IApplicationLifetime

    public class BootstrappingFixture : IDisposable
    {
        private JasperRuntime _runtime;

        public CustomHostedService theHostedService = new CustomHostedService();
        public FakeServer Server = new FakeServer();

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        public async Task<JasperRuntime> WithRuntime()
        {
            if (_runtime == null)
            {
                Server = new FakeServer();
                theHostedService = new CustomHostedService();

                _runtime = await JasperRuntime.ForAsync(_ =>
                {
                    _.Handlers.DisableConventionalDiscovery();
                    _.Services.AddTransient<IFakeStore, FakeStore>();
                    _.Services.AddTransient<IMainService, MainService>();

                    _.Services.AddSingleton<IHostedService>(theHostedService);

                    _.Services.AddSingleton<IServer>(Server);
                });
            }

            return _runtime;
        }

        public async Task Shutdown()
        {
            await _runtime.Shutdown();
            _runtime = null;
        }
    }

    [Collection("integration")]
    public class BootstrappingTests : IClassFixture<BootstrappingFixture>
    {
        private BootstrappingFixture theFixture;

        public BootstrappingTests(BootstrappingFixture fixture)
        {
            theFixture = fixture;
        }

        [Fact]
        public async Task can_determine_the_application_assembly()
        {
            var runtime = await theFixture.WithRuntime();

            runtime.ApplicationAssembly.ShouldBe(GetType().Assembly);
        }

        [Fact]
        public async Task has_the_hosted_environment()
        {
            var runtime = await theFixture.WithRuntime();

            runtime.Container.ShouldHaveRegistration<IHostingEnvironment, HostingEnvironment>();
        }

        [Fact]
        public async Task can_use_custom_hosted_service_without_aspnet()
        {
            await theFixture.WithRuntime();

            theFixture.theHostedService.WasStarted.ShouldBeTrue();
            theFixture.theHostedService.WasStopped.ShouldBeFalse();

            await theFixture.Shutdown();

            theFixture.theHostedService.WasStopped.ShouldBeTrue();

        }

        [Fact]
        public async Task registrations_from_the_main_registry_are_applied()
        {
            var runtime = await theFixture.WithRuntime();

            runtime.Container.DefaultRegistrationIs<IMainService, MainService>();
        }

        [Fact]
        public async Task starts_and_stops_the_server()
        {
            var runtime = await theFixture.WithRuntime();

            var server = theFixture.Server;


            server.WasStarted.ShouldBeTrue();
            server.WasStopped.ShouldBeFalse();

            await theFixture.Shutdown();

            server.WasStopped.ShouldBeTrue();
        }


    }

    public class FakeServer : IServer
    {
        public void Dispose()
        {

        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public bool WasStarted { get; set; }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }

        public bool WasStopped { get; set; }

        public IFeatureCollection Features { get; } = new FeatureCollection();
    }

    public class when_bootstrapping_a_runtime_with_multiple_features : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly JasperRuntime theRuntime;

        public when_bootstrapping_a_runtime_with_multiple_features()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();

            theRegistry.Services.AddTransient<IMainService, MainService>();

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();


            theRuntime = JasperRuntime.For(theRegistry);
        }

        public void Dispose()
        {
            theRuntime?.Dispose();
        }
    }

    public class when_shutting_down_the_runtime
    {
        private readonly MainService mainService = new MainService();

        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly JasperRuntime theRuntime;

        public when_shutting_down_the_runtime()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();
            theRegistry.Services.AddSingleton<IMainService>(mainService);

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();



            theRuntime = JasperRuntime.For(theRegistry);

            theRuntime.Dispose();
        }
    }


    public interface IMainService : IDisposable
    {
    }

    public class MainService : IMainService
    {
        public bool WasDisposed { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public class CustomHostedService : IHostedService
    {
        public bool WasStarted { get; set; }

        public bool WasStopped { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }
    }
}
