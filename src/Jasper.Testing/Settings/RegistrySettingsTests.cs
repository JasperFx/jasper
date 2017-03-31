using System.ComponentModel.DataAnnotations;
using Jasper.Configuration;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;
using Xunit;
using Shouldly;
using StructureMap;

namespace Jasper.Testing.Settings
{
    public class RegistrySettingsTests
    {
        private readonly JasperSettings _settings;
        private readonly JasperRegistry _registry;

        public RegistrySettingsTests()
        {
            _registry = new JasperRegistry();
            _settings = new JasperSettings(_registry);
        }

        [Fact]
        public void can_resolve_registered_types()
        {
            _settings.Build(_ =>
            {
                _.AddJsonFile("appsettings.json");
            });

            _settings.Bootstrap();
            var container = new Container(_registry.Services);
            var settings = container.GetInstance<MySettings>();
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
            var runtime = JasperRuntime.Basic();
            var registry = runtime.Registry;
            var container = new Container(registry.Services);
            var settings = container.GetInstance<MySettings>();
            settings.SomeSetting.ShouldBe(0);
        }

        [Fact]
        public void can_alter_and_registry_still_gets_defaults()
        {
            var app = new MyApp();
            app.Settings.Build(_ =>
            {
                _.AddJsonFile("appsettings.json");
                _.AddJsonFile("colors.json");
            });

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
    }
}