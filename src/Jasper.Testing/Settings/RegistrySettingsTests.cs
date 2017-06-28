using System;
using Jasper.Bus;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.Http;
using Microsoft.Extensions.Configuration;
using Xunit;
using Shouldly;
using StructureMap;

namespace Jasper.Testing.Settings
{
    public class RegistrySettingsTests : IDisposable
    {
        private JasperBusRegistry theRegistry;
        private JasperRuntime _runtime;

        public RegistrySettingsTests()
        {
            theRegistry = new JasperBusRegistry();
            theRegistry.Handlers.ExcludeTypes(x => true);
        }

        private T get<T>()
        {
            if (_runtime == null)
            {
                _runtime = JasperRuntime.For(theRegistry);
            }

            return _runtime.Container.GetInstance<T>();
        }

        [Fact]
        public void can_resolve_registered_types()
        {
            theRegistry.Configuration.AddJsonFile("appsettings.json");


            var settings = get<MySettings>();
            settings.SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_modify_registry()
        {
            var app = new MyApp();
            var runtime = JasperRuntime.For(app);
            var myApp = (MyApp) runtime.Registry;
            myApp.MySetting.ShouldBe(true);
        }

        [Fact]
        public void settings_policy_registers_settings()
        {
            var runtime = JasperRuntime.For(_ =>
            {
                _.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();
                _.Services.For<IWidget>().Use<Widget>();
                _.Services.For<IFakeService>().Use<FakeService>();
            });

            var registry = runtime.Registry;
            registry.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();
            registry.Services.For<IWidget>().Use<Widget>();
            registry.Services.For<IFakeService>().Use<FakeService>();


            var container = new Container(registry.Services);
            var settings = container.GetInstance<MySettings>();
            settings.SomeSetting.ShouldBe(0);
        }

        [Fact]
        public void can_alter_and_registry_still_gets_defaults()
        {
            var app = new MyApp();
            app.Configuration.AddJsonFile("appsettings.json")
                .AddJsonFile("colors.json");

            app.Settings.Configure<Colors>();
            app.Settings.Alter<MySettings>(_ =>
            {
                _.SomeSetting = 29;
            });

            var runtime = JasperRuntime.For(app);
            var container = new Container(runtime.Registry.Services);
            var mySettings = container.GetInstance<MySettings>();
            var colors = container.GetInstance<Colors>();

            mySettings.SomeSetting.ShouldBe(29);
            colors.Red.ShouldBe("#ff0000");
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}
