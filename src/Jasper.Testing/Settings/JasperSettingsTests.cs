using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Testing.EnvironmentChecks;
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

        private async Task with<T>(Action<T> action)
        {
            var runtime = await JasperRuntime.ForAsync(theRegistry);

            var service = runtime.Get<T>();

            try
            {
                action(service);
            }
            finally
            {
                await runtime.Shutdown();
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


                Configuration
                    .AddInMemoryCollection(new Dictionary<string, string> {{"AppName", "WocketInMyPocket"}});

                Settings.Configure(c => ServiceName = c.Configuration["AppName"]);
            }
        }

        [Fact]
        public Task can_alter_settings()
        {
            theRegistry.Settings.Alter<MyFakeSettings>(s => { s.SomeSetting = 5; });


            return with<MyFakeSettings>(x => x.SomeSetting.ShouldBe(5));
        }

        [Fact]
        public Task can_apply_alterations_using_the_config()
        {
            theRegistry.Configuration.AddJsonFile("appsettings.json");
            theRegistry.Settings.Alter<FakeSettings>((c, x) => { x.SomeSetting = int.Parse(c.Configuration["SomeSetting"]); });

            return with<FakeSettings>(x => x.SomeSetting.ShouldBe(1));
        }

        [Fact]
        public async Task can_configure_builder()
        {
            theRegistry.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile("colors.json");


            theRegistry.Settings.Require<Colors>();
            theRegistry.Settings.Require<MyFakeSettings>();

            var runtime = await JasperRuntime.ForAsync(theRegistry);

            try
            {
                var colors = runtime.Get<Colors>();
                var settings = runtime.Get<MyFakeSettings>();

                colors.Red.ShouldBe("#ff0000");
                settings.SomeSetting.ShouldBe(1);
            }
            finally
            {
                await runtime.Shutdown();
            }



        }
        // ENDSAMPLE

        [Fact]
        public Task can_configure_settings()
        {
            theRegistry.Configuration.AddJsonFile("nested.json");

            theRegistry.Settings.Configure<Colors>(_ => _.GetSection("NestedSettings"));


            return with<Colors>(colors => colors.Red.ShouldBe("#ff0000"));
        }

        [Fact]
        public Task can_configure_settings_with_the_syntactical_sugure()
        {
            theRegistry.Configuration.AddJsonFile("nested.json");
            theRegistry.Settings.BindToConfigSection<Colors>("NestedSettings");


            return with<Colors>(colors => colors.Red.ShouldBe("#ff0000"));
        }
        // ENDSAMPLE

        // SAMPLE: can_customize_based_on_only_configuration
        [Fact]
        public async Task can_customize_based_on_only_configuration()
        {
            var runtime = await JasperRuntime.ForAsync<UsingConfigApp>();

            try
            {
                runtime.ServiceName.ShouldBe("WocketInMyPocket");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public Task can_read_settings()
        {
            theRegistry.Configuration.AddJsonFile("appsettings.json");
            theRegistry.Settings.Require<MyFakeSettings>();

            return with<MyFakeSettings>(settings => settings.SomeSetting.ShouldBe(1));
        }

        [Fact]
        public Task can_replace_settings()
        {
            theRegistry.Settings.Replace(new MyFakeSettings
            {
                OtherSetting = "tacos",
                SomeSetting = 1000
            });

            return with<MyFakeSettings>(settings =>
            {
                settings.SomeSetting.ShouldBe(1000);
                settings.OtherSetting.ShouldBe("tacos");
            });
        }
    }
}
