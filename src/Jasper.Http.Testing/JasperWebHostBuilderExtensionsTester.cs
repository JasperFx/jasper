using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.EnvironmentChecks;
using Jasper.Http;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http
{
    public class AspNetCoreAppFixture : IDisposable
    {
        public readonly HttpClient client = new HttpClient();
        public RecordingEnvironmentCheck theCheck;

        public AspNetCoreAppFixture()
        {
            theCheck = new RecordingEnvironmentCheck();

            var builder = new WebHostBuilder();
            builder.UseUrls("http://localhost:3456");
            builder.UseKestrel();
            builder.ConfigureServices(x =>
            {
                x.AddSingleton<IService, Service>();
                x.AddSingleton<IEnvironmentCheck>(theCheck);
            });

            builder.ConfigureAppConfiguration(b =>
            {
                b.AddInMemoryCollection(new Dictionary<string, string> {{"city", "Austin"}});
            });


            builder.UseStartup<AppStartUp>();
            builder.UseEnvironment("Green");
            builder.UseJasper<BootstrappingApp>();

            theHost = builder.Build();
            theHost.Start();
        }

        public IWebHost theHost { get; }

        public void Dispose()
        {
            theHost.Dispose();
            client.Dispose();
        }
    }


    public class FakeHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class ServiceFromStartup : IService
    {
    }

    public class AppStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IService, ServiceFromStartup>();
            services.AddTransient<IHostedService, FakeHostedService>();
        }

        public void Configure(IApplicationBuilder builder)
        {
            builder.MapWhen(
                c => c.Request.Path.Value.Contains("startup"),
                b => b.Run(c => c.Response.WriteAsync("from startup route")));
        }
    }

    public class RecordingEnvironmentCheck : IEnvironmentCheck
    {
        public bool DidAssert { get; set; }

        public void Assert(JasperRuntime runtime)
        {
            ShouldBeNullExtensions.ShouldNotBeNull(runtime);
            DidAssert = true;
        }

        public string Description => "Just Recording";
    }

    public class CheckEndpoint
    {
        public string get_check()
        {
            return "got this from jasper route";
        }
    }


    public class BootstrappingToken
    {
        public BootstrappingToken(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GreenService : IService
    {
    }

    public class BootstrappingApp : JasperRegistry
    {
        public static readonly Guid Id = Guid.NewGuid();

        public BootstrappingApp()
        {
            Services.For<BootstrappingToken>().Use(new BootstrappingToken(Id));

            Configuration.AddInMemoryCollection(new Dictionary<string, string> {{"foo", "bar"}});

            Hosting.ConfigureAppConfiguration(c =>
                c.AddInMemoryCollection(new Dictionary<string, string> {{"team", "chiefs"}}));

            Hosting.ConfigureServices(s => s.AddTransient<IService, ServiceFromJasperRegistryConfigure>());

            Hosting.ConfigureServices((c, services) =>
            {
                if (c.HostingEnvironment.EnvironmentName == "Green") services.AddTransient<IService, GreenService>();
            });

            Hosting.Configure(app =>
            {
                app.MapWhen(
                    c => c.Request.Path.Value.Contains("host"),
                    b => b.Run(c => c.Response.WriteAsync("from jasperregistry host")));
            });

            Settings.Alter<BootstrappingSetting>((context, settings) =>
            {
                settings.Environment = context.HostingEnvironment.EnvironmentName;
                settings.Team = context.Configuration["team"];
                settings.City = context.Configuration["city"];
            });
        }
    }

    public class BootstrappingSetting
    {
        public string Environment { get; set; }
        public string Team { get; set; }
        public string City { get; set; }
    }

    public class JasperWebHostBuilderExtensionsTester : IClassFixture<AspNetCoreAppFixture>
    {
        public JasperWebHostBuilderExtensionsTester(AspNetCoreAppFixture fixture)
        {
            theHost = fixture.theHost;
            theContainer = theHost.Services.As<IContainer>();
            theClient = fixture.client;
            theCheck = fixture.theCheck;
        }

        private readonly IWebHost theHost;
        private readonly IContainer theContainer;
        private readonly HttpClient theClient;
        private readonly RecordingEnvironmentCheck theCheck;
/*
        [Fact]
        public async Task applies_jasper_router_too()
        {
            var server = theContainer.GetInstance<IServer>();

            var text = await theClient.GetStringAsync("http://localhost:3456/check");
            text.ShouldBe("got this from jasper route");
        }

        [Fact]
        public void apply_changes_to_settings_with_combined_configuration_and_host()
        {
            var settings = theContainer.GetInstance<BootstrappingSetting>();
            settings.Team.ShouldBe("chiefs");
            settings.City.ShouldBe("Austin");
            settings.Environment.ShouldBe("Green");
        }

        [Fact]
        public void environment_checks_run()
        {
            theCheck.DidAssert.ShouldBeTrue();
        }

        [Fact]
        public async Task gets_app_builder_configuration_from_aspnet_startup()
        {
            var text = await theClient.GetStringAsync("http://localhost:3456/startup");
            text.ShouldBe("from startup route");
        }


        [Fact]
        public async Task gets_app_builder_configuration_from_jasper_registry_host_calls()
        {
            var text = await theClient.GetStringAsync("http://localhost:3456/host");
            text.ShouldBe("from jasperregistry host");
        }

        [Fact]
        public void gets_configuration_from_JasperRegistry_Configuration()
        {
            theHost.Services.GetService<IConfiguration>()["foo"].ShouldBe("bar");
        }

        [Fact]
        public void gets_configuration_from_JasperRegistry_Configure_methods()
        {
            theContainer.GetInstance<IConfiguration>()["team"].ShouldBe("chiefs");
        }

        [Fact]
        public void gets_service_registrations_from_aspnet_core_startup()
        {
            theContainer.GetAllInstances<IService>().OfType<ServiceFromStartup>()
                .Any().ShouldBeTrue();
        }

        [Fact]
        public void has_handlers()
        {
            theContainer.GetInstance<HandlerGraph>().Chains.Any().ShouldBeTrue();
        }

        [Fact]
        public void has_message_activator_before_other_activators()
        {
            theContainer.Model.For<IHostedService>().Instances.First()
                .ImplementationType.ShouldBe(typeof(MessagingActivator));
        }

        [Fact]
        public void has_service_registrations_from_jasper()
        {
            theHost.Services.GetService<BootstrappingToken>()
                .Id.ShouldBe(BootstrappingApp.Id);
        }


        [Fact]
        public void has_service_registrations_from_outside_of_jasper()
        {
            theContainer.Model.For<IService>().Instances
                .Any(x => x.ImplementationType == typeof(Service))
                .ShouldBeTrue();
        }

        [Fact]
        public void is_using_lamar_for_the_service_provider()
        {
            theHost.Services.ShouldBeOfType<Container>();
        }

        [Fact]
        public void jasper_runtime_is_registered()
        {
            theContainer.Model.HasRegistrationFor<JasperRuntime>()
                .ShouldBeTrue();
        }

        [Fact]
        public void should_get_services_from_JasperRegistry_Host_ConfigureServices()
        {
            theContainer.Model.For<IService>().Instances
                .Any(x => x.ImplementationType == typeof(ServiceFromJasperRegistryConfigure))
                .ShouldBeTrue();
        }


        [Fact]
        public void uses_environment_name_from_host_in_jasper_registry_registrations()
        {
            theContainer.Model.For<IService>().Instances
                .Any(x => x.ImplementationType == typeof(GreenService))
                .ShouldBeTrue();
        }
        */
    }

    public class Service : IService
    {
    }

    public class ServiceFromJasperRegistryConfigure : IService
    {
    }


    public interface IService
    {
    }
}
