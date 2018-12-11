using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alba;
using Baseline;
using Jasper;
using Jasper.EnvironmentChecks;
using Jasper.Http;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    public class AspNetCoreIntegrationFixture : Fixture
    {
        private RecordingEnvironmentCheck theCheck;
        private IContainer theContainer;
        private SystemUnderTest theSystem;


        public AspNetCoreIntegrationFixture()
        {
            Title = "ASP.Net Core Integration";
        }

        public override void SetUp()
        {
            theCheck = new RecordingEnvironmentCheck();

            var builder = new WebHostBuilder();
            builder.UseUrls("http://localhost:3456");
            //builder.UseKestrel();
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


            theSystem = new SystemUnderTest(builder);



            theContainer = theSystem.Services.As<IContainer>();
        }

        public override void TearDown()
        {
            theSystem.Dispose();
        }

        public IGrammar SettingsShouldBe()
        {
            return VerifyPropertiesOf<BootstrappingSetting>(
                "1.) The resolved properties of BootstrappingSettings should be",
                x =>
                {
                    x.Object = () => theSystem.Services.GetRequiredService<BootstrappingSetting>();
                    x.Check(_ => _.Team);
                    x.Check(_ => _.City);
                    x.Check(_ => _.Environment);
                });
        }

        [FormatAs("2.) The environment checks ran during bootstrapping")]
        public bool TheEnvironmentChecksRan()
        {
            return theCheck.DidAssert;
        }

        [FormatAs("3.) Can hit Jasper routes")]
        public Task CanHitJasperRoutes()
        {
            return theSystem.Scenario(x =>
            {
                x.Get.Url("/check");
                x.ContentShouldContain("got this from jasper route");
            });
        }

        [FormatAs("4.) Can hit ASP.Net Core routes")]
        public Task CanHitAspNetCoreRoutes()
        {
            return theSystem.Scenario(x =>
            {
                x.Get.Url("/startup");
                x.ContentShouldContain("from startup route");
            });
        }

        [FormatAs("5.) Gets AppBuilder Configuration from JasperRegistry Host Calls")]
        public Task GetsAppBuilderConfigurationFromJasperRegistryHostCalls()
        {
            return theSystem.Scenario(x =>
            {
                x.Get.Url("/host");
                x.ContentShouldContain("from jasperregistry host");
            });
        }

        [FormatAs("6.) Configuration[{key}] should be {value}")]
        public string GetKey(string key)
        {
            return theSystem.Services.GetService<IConfiguration>()[key];
        }

        [FormatAs("7.) Has message handlers")]
        public bool HasHandlers()
        {
            return theSystem.Services.GetService<HandlerGraph>().Chains.Any();
        }

        [FormatAs("9.) Has service registrations from Jasper")]
        public bool HasServiceRegistrationsFromJasper()
        {
            return theSystem.Services.GetService<BootstrappingToken>()
                       .Id == BootstrappingApp.Id;
        }

        [FormatAs("10.) Has service registrations from outside of Jasper")]
        public bool HasServiceRegistrationsFromOutsideOfJasper()
        {
            return theContainer.Model.For<IService>().Instances
                .Any(x => x.ImplementationType == typeof(Service));
        }

        [FormatAs("11.) Uses Lamar for the ServiceProvider")]
        public bool is_using_lamar_for_the_service_provider()
        {
            return theSystem.Services.GetType() == typeof(Container);
        }

        [FormatAs("12.) JasperRuntime is registered")]
        public bool jasper_runtime_is_registered()
        {
            return theContainer.Model.HasRegistrationFor<JasperRuntime>();
        }

        [FormatAs("13.) Should have services from JasperRegistry Host ConfigureServices call")]
        public bool should_get_services_from_JasperRegistry_Host_ConfigureServices()
        {
            return theContainer.Model.For<IService>().Instances
                .Any(x => x.ImplementationType == typeof(ServiceFromJasperRegistryConfigure));
        }


        [FormatAs("14.) Uses the environment name from host")]
        public bool uses_environment_name_from_host_in_jasper_registry_registrations()
        {
            return theContainer.Model.For<IService>().Instances
                .Any(x => x.ImplementationType == typeof(GreenService));
        }

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
            StoryTellerAssert.Fail(runtime == null, "runtime should not be null");

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

            Hosting.ConfigureAppConfiguration(c =>
            {
                c.AddInMemoryCollection(new Dictionary<string, string> {{"foo", "bar"}});
                c.AddInMemoryCollection(new Dictionary<string, string> {{"team", "chiefs"}});
            });

            Hosting.ConfigureServices(s => s.AddTransient<IService, ServiceFromJasperRegistryConfigure>());

            Hosting.ConfigureServices((c, services) =>
            {
                if (c.HostingEnvironment.EnvironmentName == "Green") services.AddTransient<IService, GreenService>();
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
}
