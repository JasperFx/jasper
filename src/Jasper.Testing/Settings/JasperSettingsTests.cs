using System.ComponentModel.DataAnnotations;
using Jasper.Configuration;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;
using Xunit;
using Shouldly;
using NSubstitute;
using StructureMap;

namespace Jasper.Testing.Settings
{
    public class JasperSettingsTests
    {
        private readonly JasperSettings _settings;
        private readonly ServiceRegistry _registry;

        public JasperSettingsTests()
        {
            _settings = new JasperSettings();
            _registry = new ServiceRegistry();
        }

        [Fact]
        public void can_read_settings()
        {
            _settings.Configure<MySettings>();
            _settings.Bootstrap(_registry);
            var settings = _settings.Get<MySettings>();
            Assert.Equal(settings.SomeSetting, 1);
        }

        [Fact]
        public void can_alter_settings()
        {
            _settings.Configure<MySettings>();
            _settings.Alter<MySettings>(s =>
            {
                s.SomeSetting = 5;
            });

            _settings.Bootstrap(_registry);
            var settings = _settings.Get<MySettings>();

            Assert.Equal(settings.SomeSetting, 5);
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

            _settings.Bootstrap(_registry);
            var settings = _settings.Get<MySettings>();

            Assert.Equal(settings.SomeSetting, 1000);
            Assert.Equal(settings.OtherSetting, "tacos");
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
            _settings.Configure<Colors>();
            _settings.Bootstrap(_registry);

            var colors = _settings.Get<Colors>();
            var settings = _settings.Get<MySettings>();

            Assert.Equal(colors.Red, "#ff0000");
            Assert.Equal(settings.SomeSetting, 1);
        }

        [Fact]
        public void can_resolve_registered_types()
        {
            _settings.Configure<MySettings>();
            _settings.Bootstrap(_registry);
            var container = new Container(_registry);
            var settings = container.GetInstance<MySettings>();
            Assert.Equal(settings.SomeSetting, 1);
        }
    }
}