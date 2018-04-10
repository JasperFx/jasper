using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Testing.Messaging.Bootstrapping;
using Module1;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class discovering_and_using_extensions
    {


        [Fact]
        public async Task application_service_registrations_win()
        {
            var runtime = await JasperRuntime.ForAsync<AppWithOverrides>();

            try
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
            finally
            {
                await runtime.Shutdown();
            }



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
