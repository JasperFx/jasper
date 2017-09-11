using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Jasper.Bus;
using Jasper.Configuration;
using Jasper.Settings;
using Microsoft.Extensions.Configuration;
using Xunit;
using Shouldly;
using StructureMap;

namespace Jasper.Testing.Settings
{
    public class JasperSettingsTests : IDisposable
    {
        private JasperRegistry theRegistry;
        private JasperRuntime _runtime;

        public JasperSettingsTests()
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
        public void can_read_settings()
        {
            theRegistry.Configuration.AddJsonFile("appsettings.json");


            var settings = get<MyFakeSettings>();
            settings.SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_apply_alterations_using_the_config()
        {
            theRegistry.Configuration.AddJsonFile("appsettings.json");
            theRegistry.Settings.Alter<FakeSettings>((c, x) =>
            {
                x.SomeSetting = int.Parse(c["SomeSetting"]);
            });

            get<FakeSettings>().SomeSetting.ShouldBe(1);
        }

        public class FakeSettings
        {
            public int SomeSetting { get; set; }
        }

        [Fact]
        public void can_alter_settings()
        {
            theRegistry.Settings.Alter<MyFakeSettings>(s =>
            {
                s.SomeSetting = 5;
            });

            var settings = get<MyFakeSettings>();

            settings.SomeSetting.ShouldBe(5);
        }

        [Fact]
        public void can_replace_settings()
        {

            theRegistry.Settings.Replace(new MyFakeSettings
            {
                OtherSetting = "tacos",
                SomeSetting = 1000
            });

            var settings = get<MyFakeSettings>();

            settings.SomeSetting.ShouldBe(1000);
            settings.OtherSetting.ShouldBe("tacos");
        }

        [Fact]
        public void can_configure_builder()
        {
            theRegistry.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile("colors.json");

            theRegistry.Settings.Configure<Colors>();
            var colors = get<Colors>();
            var settings = get<MyFakeSettings>();

            colors.Red.ShouldBe("#ff0000");
            settings.SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_configure_settings()
        {
            theRegistry.Configuration.AddJsonFile("nested.json");

            theRegistry.Settings.Configure<Colors>(_ => _.GetSection("NestedSettings"));

            var colors = get<Colors>();

            colors.Red.ShouldBe("#ff0000");
        }

        [Fact]
        public void can_configure_settings_with_the_syntactical_sugure()
        {
            theRegistry.Configuration.AddJsonFile("nested.json");
            theRegistry.Settings.BindToConfigSection<Colors>("NestedSettings");

            var colors = get<Colors>();

            colors.Red.ShouldBe("#ff0000");
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}
