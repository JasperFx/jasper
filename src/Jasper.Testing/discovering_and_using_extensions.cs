using System;
using System.Linq;
using Jasper.Bus.Logging;
using Jasper.Testing.Bus.Bootstrapping;
using Module1;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{

    public class discovering_and_using_extensions : IDisposable
    {
        private JasperRuntime theRuntime;

        public discovering_and_using_extensions()
        {
            theRuntime = JasperRuntime.For<AppWithOverrides>();
        }

        public void Dispose()
        {
            theRuntime.Dispose();
        }



        [Fact]
        public void can_inject_services_from_the_extension()
        {
            theRuntime.Services.Any(x =>
                    x.ServiceType == typeof(IBusLogger) && x.ImplementationType == typeof(ModuleBusLogger))
                .ShouldBeTrue();

        }

        [Fact]
        public void application_service_registrations_win()
        {
            theRuntime.Container.DefaultRegistrationIs<IModuleService, AppsModuleService>();
        }

        [Fact]
        public void extension_can_alter_settings()
        {
            // This value comes from Module1Extension
            var moduleSettings = theRuntime.Get<ModuleSettings>();
            moduleSettings
                .Count.ShouldBe(100);
        }

        [Fact]
        public void application_settings_alterations_win()
        {
            theRuntime.Get<ModuleSettings>()
                .From.ShouldBe("Application");
        }
    }



    public class AppWithOverrides : JasperRegistry
    {
        public AppWithOverrides()
        {
            Handlers.DisableConventionalDiscovery(true);

            Settings.Alter<ModuleSettings>(_ => _.From = "Application");

            Services.For<IModuleService>().Use<AppsModuleService>();


        }
    }

    public class AppsModuleService : IModuleService{}



}
