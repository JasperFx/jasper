using System.ComponentModel.DataAnnotations;
using Jasper.Configuration;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;
using Xunit;
using Shouldly;
using StructureMap;

namespace Jasper.Testing.Settings
{
    public class JasperSettingsTests
    {
        private readonly JasperSettings _settings;
        private readonly JasperRegistry _registry;

        public JasperSettingsTests()
        {
            _registry = new JasperRegistry();
            _settings = new JasperSettings(_registry);
        }

        [Fact]
        public void can_read_settings()
        {
            _settings.Configure<MySettings>();
            _settings.Bootstrap();
            var settings = _settings.Get<MySettings>();
            settings.SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_alter_settings()
        {
            _settings.Configure<MySettings>();
            _settings.Alter<MySettings>(s =>
            {
                s.SomeSetting = 5;
            });

            _settings.Bootstrap();
            var settings = _settings.Get<MySettings>();

            settings.SomeSetting.ShouldBe(5);
        }

        [Fact]
        public void can_replace_settings()
        {
            _settings.Configure<MySettings>();
            _settings.Replace(new MySettings
            {
                OtherSetting = "tacos",
                SomeSetting = 1000
            });

            _settings.Bootstrap();
            var settings = _settings.Get<MySettings>();

            settings.SomeSetting.ShouldBe(1000);
            settings.OtherSetting.ShouldBe("tacos");
        }

        [Fact]
        public void can_configure_builder()
        {
            _settings.Build(_ =>
            {
                _.AddJsonFile("appsettings.json");
                _.AddJsonFile("colors.json");
            });

            _settings.Configure<MySettings>();
            _settings.Configure<ColorSettings>();
            _settings.Bootstrap();

            var colors = _settings.Get<ColorSettings>();
            var settings = _settings.Get<MySettings>();

            colors.Red.ShouldBe("#ff0000");
            settings.SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_configure_settings()
        {
            _settings.Build(_ =>
            {
                _.AddJsonFile("nested.json");
            });

            _settings.Configure<ColorSettings>(_ => _.GetSection("NestedSettings"));

            _settings.Bootstrap();

            var colors = _settings.Get<ColorSettings>();

            colors.Red.ShouldBe("#ff0000");
        }

        [Fact]
        public void can_resolve_registered_types()
        {
            _settings.Configure<MySettings>();
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
            settings.SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_alter_and_registry_still_gets_defaults()
        {
            var app = new MyApp();
            app.Settings.Build(_ =>
            {
                _.AddJsonFile("appsettings.config");
                _.AddJsonFile("colors.json");
            });

            app.Settings.Alter<MySettings>(_ =>
            {
                _.SomeSetting = 29;
            });

            var runtime = JasperRuntime.For(app);
            var container = new Container(runtime.Registry.Services);
            var mySettings = container.GetInstance<MySettings>();
            var colors = container.GetInstance<ColorSettings>();

            mySettings.SomeSetting.ShouldBe(29);
            colors.Red.ShouldBe("#ff0000");
        }
    }
}