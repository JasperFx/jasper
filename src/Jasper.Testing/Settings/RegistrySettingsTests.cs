using System;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Settings
{
    public class RegistrySettingsTests : IDisposable
    {
        public RegistrySettingsTests()
        {
            theRegistry = new JasperRegistry();
            theRegistry.Handlers.ExcludeTypes(x => true);
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        private readonly JasperRegistry theRegistry;
        private JasperRuntime _runtime;

        private T get<T>()
        {
            if (_runtime == null) _runtime = JasperRuntime.For(theRegistry);

            return _runtime.Get<T>();
        }

        [Fact]
        public void can_alter_and_registry_still_gets_defaults()
        {
            var app = new MyApp();
            app.Configuration.AddJsonFile("appsettings.json")
                .AddJsonFile("colors.json");

            app.Settings.Require<Colors>();
            app.Settings.Alter<MyFakeSettings>(_ => { _.SomeSetting = 29; });

            using (var runtime = JasperRuntime.For(app))
            {
                var mySettings = runtime.Get<MyFakeSettings>();
                var colors = runtime.Get<Colors>();

                mySettings.SomeSetting.ShouldBe(29);
                colors.Red.ShouldBe("#ff0000");
            }
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


            var settings = runtime.Get<MyFakeSettings>();
            settings.SomeSetting.ShouldBe(0);
        }
    }
}
