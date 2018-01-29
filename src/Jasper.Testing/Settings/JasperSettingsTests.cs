using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Settings
{
    public class JasperSettingsTests : IDisposable
    {
        public JasperSettingsTests()
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

        public class FakeSettings
        {
            public int SomeSetting { get; set; }
        }


        // SAMPLE: UsingConfigApp
        public class UsingConfigApp : JasperRegistry
        {
            public UsingConfigApp()
            {
                // Ignore this please;)
                Handlers.DisableConventionalDiscovery();


                Configuration
                    .AddInMemoryCollection(new Dictionary<string, string> {{"AppName", "WocketInMyPocket"}});

                Settings.WithConfig(c => ServiceName = c["AppName"]);
            }
        }

        [Fact]
        public void can_alter_settings()
        {
            theRegistry.Settings.Alter<MyFakeSettings>(s => { s.SomeSetting = 5; });

            var settings = get<MyFakeSettings>();

            settings.SomeSetting.ShouldBe(5);
        }

        [Fact]
        public void can_apply_alterations_using_the_config()
        {
            theRegistry.Configuration.AddJsonFile("appsettings.json");
            theRegistry.Settings.Alter<FakeSettings>((c, x) => { x.SomeSetting = int.Parse(c["SomeSetting"]); });

            get<FakeSettings>().SomeSetting.ShouldBe(1);
        }

        [Fact]
        public void can_configure_builder()
        {
            theRegistry.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile("colors.json");

            theRegistry.Settings.Require<Colors>();
            theRegistry.Settings.Require<MyFakeSettings>();
            var colors = get<Colors>();
            var settings = get<MyFakeSettings>();

            colors.Red.ShouldBe("#ff0000");
            settings.SomeSetting.ShouldBe(1);
        }
        // ENDSAMPLE

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
        // ENDSAMPLE

        // SAMPLE: can_customize_based_on_only_configuration
        [Fact]
        public void can_customize_based_on_only_configuration()
        {
            using (var runtime = JasperRuntime.For<UsingConfigApp>())
            {
                runtime.ServiceName.ShouldBe("WocketInMyPocket");
            }
        }

        [Fact]
        public void can_read_settings()
        {
            theRegistry.Configuration.AddJsonFile("appsettings.json");
            theRegistry.Settings.Require<MyFakeSettings>();

            var settings = get<MyFakeSettings>();
            settings.SomeSetting.ShouldBe(1);
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
    }
}
