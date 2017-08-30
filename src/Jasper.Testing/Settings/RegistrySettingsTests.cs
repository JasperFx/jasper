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
        private JasperRegistry theRegistry;
        private JasperRuntime _runtime;

        public RegistrySettingsTests()
        {
            theRegistry = new JasperRegistry();
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
            using (var runtime = JasperRuntime.For(app))
            {
                app.MySetting.ShouldBe(true);
            }
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


            var settings = runtime.Get<MySettings>();
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

            using (var runtime = JasperRuntime.For(app))
            {
                var mySettings = runtime.Get<MySettings>();
                var colors = runtime.Get<Colors>();

                mySettings.SomeSetting.ShouldBe(29);
                colors.Red.ShouldBe("#ff0000");
            }
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}
