using System;
using System.Threading.Tasks;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Messaging.Compilation;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Settings
{
    public class RegistrySettingsTests
    {
        [Fact]
        public async Task can_alter_and_registry_still_gets_defaults()
        {
            var app = new MyApp();
            app.Configuration.AddJsonFile("appsettings.json")
                .AddJsonFile("colors.json");

            app.Settings.Require<Colors>();
            app.Settings.Alter<MyFakeSettings>(_ => { _.SomeSetting = 29; });

            var runtime = await JasperRuntime.ForAsync(app);

            try
            {
                var mySettings = runtime.Get<MyFakeSettings>();
                var colors = runtime.Get<Colors>();

                mySettings.SomeSetting.ShouldBe(29);
                colors.Red.ShouldBe("#ff0000");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task can_modify_registry()
        {
            var app = new MyApp();

            var runtime = await JasperRuntime.ForAsync(app);

            try
            {
                app.MySetting.ShouldBe(true);
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task settings_policy_registers_settings()
        {
            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();
            });


            try
            {
                var settings = runtime.Get<MyFakeSettings>();
                settings.SomeSetting.ShouldBe(0);
            }
            finally
            {
                await runtime.Shutdown();
            }


        }
    }
}
