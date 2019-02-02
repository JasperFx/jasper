using Jasper;
using Module1;
using Shouldly;
using TestingSupport;
using Xunit;

namespace CoreTests.Bootstrapping
{
    public class discovering_and_using_extensions
    {
        [Fact]
        public void application_service_registrations_win()
        {
            using (var runtime = JasperHost.For<AppWithOverrides>())
            {
                runtime.Container.DefaultRegistrationIs<IModuleService, AppsModuleService>();

                // application_settings_alterations_win
                runtime.Get<ModuleSettings>()
                    .From.ShouldBe("Application");

                // extension_can_alter_settings
                var moduleSettings = runtime.Get<ModuleSettings>();
                moduleSettings
                    .Count.ShouldBe(100);
            }

        }
    }


    public class AppWithOverrides : JasperRegistry
    {
        public AppWithOverrides()
        {
            Handlers.DisableConventionalDiscovery();

            Settings.Alter<ModuleSettings>(_ => _.From = "Application");

            Services.For<IModuleService>().Use<AppsModuleService>();
        }
    }

    public class AppsModuleService : IModuleService
    {
    }
}
