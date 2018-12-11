using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Settings
{
    public class JasperSettingsTests
    {
        public JasperSettingsTests()
        {
            theRegistry = new JasperRegistry();
            theRegistry.Handlers.DisableConventionalDiscovery();
        }

        private readonly JasperRegistry theRegistry;

        private void with<T>(Action<T> action)
        {
            using (var runtime = JasperRuntime.For(theRegistry))
            {
                var service = runtime.Get<T>();
                action(service);
            }
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


                Hosting.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string> {{"AppName", "WocketInMyPocket"}});
                });


                Settings.Alter<JasperOptions>((c, options) => options.ServiceName = c.Configuration["AppName"]);
            }
        }

        [Fact]
        public void can_alter_settings()
        {
            theRegistry.Settings.Alter<MyFakeSettings>(s => { s.SomeSetting = 5; });


            with<MyFakeSettings>(x => x.SomeSetting.ShouldBe(5));
        }

        [Fact]
        public void can_apply_alterations_using_the_config()
        {
            theRegistry.Hosting.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json");
            });

            theRegistry.Settings.Alter<FakeSettings>((c, x) =>
            {
                x.SomeSetting = int.Parse(c.Configuration["SomeSetting"]);
            });

            with<FakeSettings>(x => x.SomeSetting.ShouldBe(1));
        }

        [Fact]
        public async Task can_configure_builder()
        {
            theRegistry.Hosting.ConfigureAppConfiguration((_, config) =>
            {
                config
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("colors.json");
            });


            theRegistry.Settings.Require<Colors>();
            theRegistry.Settings.Require<MyFakeSettings>();

            using (var runtime = JasperRuntime.For(theRegistry))
            {
                var colors = runtime.Get<Colors>();
                var settings = runtime.Get<MyFakeSettings>();

                colors.Red.ShouldBe("#ff0000");
                settings.SomeSetting.ShouldBe(1);
            }

        }
        // ENDSAMPLE

        [Fact]
        public void can_configure_settings()
        {
            theRegistry.Hosting.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("nested.json");
            });

            theRegistry.Settings.Configure<Colors>(_ => _.GetSection("NestedSettings"));


            with<Colors>(colors => colors.Red.ShouldBe("#ff0000"));
        }

        [Fact]
        public void can_configure_settings_with_the_syntactical_sugure()
        {
            theRegistry.Hosting.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("nested.json");
            });

            theRegistry.Settings.BindToConfigSection<Colors>("NestedSettings");


            with<Colors>(colors => colors.Red.ShouldBe("#ff0000"));
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
            theRegistry.Hosting.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json");
            });

            theRegistry.Settings.Require<MyFakeSettings>();

            with<MyFakeSettings>(settings => settings.SomeSetting.ShouldBe(1));
        }

        [Fact]
        public void can_replace_settings()
        {
            theRegistry.Settings.Replace(new MyFakeSettings
            {
                OtherSetting = "tacos",
                SomeSetting = 1000
            });

            with<MyFakeSettings>(settings =>
            {
                settings.SomeSetting.ShouldBe(1000);
                settings.OtherSetting.ShouldBe("tacos");
            });
        }
    }
}
