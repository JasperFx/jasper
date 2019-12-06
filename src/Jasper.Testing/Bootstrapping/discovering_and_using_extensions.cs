using Lamar;
using Module1;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class discovering_and_using_extensions
    {
        [Fact]
        public void application_service_registrations_win()
        {
            using (var runtime = JasperHost.For<AppWithOverrides>())
            {
                runtime.Get<IContainer>().DefaultRegistrationIs<IModuleService, AppsModuleService>();

            }
        }
    }


    public class AppWithOverrides : JasperOptions
    {
        public AppWithOverrides()
        {
            Handlers.DisableConventionalDiscovery();

            Services.For<IModuleService>().Use<AppsModuleService>();
        }
    }

    public class AppsModuleService : IModuleService
    {
    }
}
