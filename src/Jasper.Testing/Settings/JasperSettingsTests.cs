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

        public JasperSettingsTests()
        {
            _settings = new JasperSettings(new JasperRegistry());
        }

        [Fact]
        public void can_read_settings()
        {
            _settings.Build(_ =>
            {
                _.AddJsonFile("appsettings.json");
            });

            var settings = _settings.Get<MySettings>();
            settings.SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_alter_settings()
        {
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

            _settings.Configure<Colors>();
            var colors = _settings.Get<Colors>();
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

            _settings.Configure<Colors>(_ => _.GetSection("NestedSettings"));

            var colors = _settings.Get<Colors>();

            colors.Red.ShouldBe("#ff0000");
        }
    }
}