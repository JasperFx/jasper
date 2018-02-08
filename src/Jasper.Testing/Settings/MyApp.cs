using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Settings
{
    public class MyApp : JasperRegistry
    {
        public MyApp()
        {
            Handlers.DisableConventionalDiscovery();

            Services.AddTransient<IFakeStore, FakeStore>();
            Services.For<IWidget>().Use<Widget>();
            Services.For<IFakeService>().Use<FakeService>();

            Settings.With<MyFakeSettings>(_ =>
            {
                if (_.SomeSetting != int.MaxValue) MySetting = true;
            });
        }

        public bool MySetting { get; set; }
    }
}
