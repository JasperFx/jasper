using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using TestingSupport.Fakes;
using Xunit;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace CoreTests.Bootstrapping
{
    // make collection
    // test IApplicationLifetime

    public class BootstrappingFixture : IDisposable
    {
        private IJasperHost _host;
        public FakeServer Server = new FakeServer();

        public CustomHostedService theHostedService = new CustomHostedService();

        public void Dispose()
        {
            _host?.Dispose();
        }

        public IJasperHost WithHost()
        {
            if (_host == null)
            {
                Server = new FakeServer();
                theHostedService = new CustomHostedService();

                _host = JasperHost.For(_ =>
                {
                    _.Handlers.DisableConventionalDiscovery();
                    _.Services.AddTransient<IFakeStore, FakeStore>();
                    _.Services.AddTransient<IMainService, MainService>();

                    _.Services.AddSingleton<IHostedService>(theHostedService);

                    _.Services.AddSingleton<IServer>(Server);
                });
            }

            return _host;
        }

        public void Shutdown()
        {
            _host.Dispose();
            _host = null;
        }
    }

    [Collection("integration")]
    public class BootstrappingTests : IClassFixture<BootstrappingFixture>
    {
        public BootstrappingTests(BootstrappingFixture fixture)
        {
            theFixture = fixture;
        }

        private readonly BootstrappingFixture theFixture;

        [Fact]
        public void can_determine_the_application_assembly()
        {
            var runtime = theFixture.WithHost();

            runtime.ApplicationAssembly.ShouldBe(GetType().Assembly);
        }

        [Fact]
        public void can_use_custom_hosted_service_without_aspnet()
        {
            theFixture.WithHost();

            theFixture.theHostedService.WasStarted.ShouldBeTrue();
            theFixture.theHostedService.WasStopped.ShouldBeFalse();

            theFixture.Shutdown();

            theFixture.theHostedService.WasStopped.ShouldBeTrue();
        }

        [Fact]
        public void has_the_hosted_environment()
        {
            var runtime = theFixture.WithHost();

            runtime.Container.ShouldHaveRegistration<IHostingEnvironment, HostingEnvironment>();
        }

        [Fact]
        public void registrations_from_the_main_registry_are_applied()
        {
            var runtime = theFixture.WithHost();

            runtime.Container.DefaultRegistrationIs<IMainService, MainService>();
        }

        [Fact]
        public void starts_and_stops_the_server()
        {
            var runtime = theFixture.WithHost();

            var server = theFixture.Server;


            server.WasStarted.ShouldBeTrue();
            server.WasStopped.ShouldBeFalse();

            theFixture.Shutdown();

            server.WasStopped.ShouldBeTrue();
        }
    }

    public class FakeServer : IServer
    {
        public bool WasStarted { get; set; }

        public bool WasStopped { get; set; }

        public void Dispose()
        {
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }

        public IFeatureCollection Features { get; } = new FeatureCollection();
    }

    public class when_bootstrapping_a_host_with_multiple_features : IDisposable
    {
        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly IJasperHost theHost;

        public when_bootstrapping_a_host_with_multiple_features()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();

            theRegistry.Services.AddTransient<IMainService, MainService>();

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();


            theHost = JasperHost.For(theRegistry);
        }

        public void Dispose()
        {
            theHost?.Dispose();
        }
    }

    public class when_shutting_down_the_host
    {
        private readonly MainService mainService = new MainService();

        private readonly JasperRegistry theRegistry = new JasperRegistry();

        private readonly IJasperHost theHost;

        public when_shutting_down_the_host()
        {
            theRegistry.Handlers.DisableConventionalDiscovery();
            theRegistry.Services.AddSingleton<IMainService>(mainService);

            theRegistry.Services.AddTransient<IFakeStore, FakeStore>();


            theHost = JasperHost.For(theRegistry);

            theHost.Dispose();
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
