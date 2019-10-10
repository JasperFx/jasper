using System;
using System.Collections.Generic;
using Jasper;
using Jasper.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace CoreTests.Settings
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
            using (var runtime = JasperHost.For(theRegistry))
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


                Hosting( x=> x.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string> {{"AppName", "WocketInMyPocket"}});
                }));


                Settings.Alter<JasperOptions>((c, options) => options.ServiceName = c.Configuration["AppName"]);
            }
        }
        // ENDSAMPLE

        // SAMPLE: UsingStartupForConfigurationOfSettings
        public class Startup
        {
            private readonly JasperOptions _options;
            private readonly IConfiguration _configuration;
            private readonly IHostingEnvironment _hosting;

            public Startup(JasperOptions options, IConfiguration configuration, IHostingEnvironment hosting)
            {
                _options = options;
                _configuration = configuration;
                _hosting = hosting;
            }

            public void Configure(IApplicationBuilder app)
            {
                // modify the JasperOptions with your IConfiguration
                // and IHostingEnvironment
            }


        }
        // ENDSAMPLE



        [Fact]
        public void can_alter_settings()
        {
            theRegistry.Settings.Alter<MyFakeSettings>(s => { s.SomeSetting = 5; });


            with<MyFakeSettings>(x => x.SomeSetting.ShouldBe(5));
        }

        [Fact]
        public void can_apply_alterations_using_the_config()
        {
            theRegistry.Hosting( x => x.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json");
            }));

            theRegistry.Settings.Alter<FakeSettings>((c, x) =>
            {
                x.SomeSetting = int.Parse(c.Configuration.GetSection("MyFake")["SomeSetting"]);
            });

            with<FakeSettings>(x => x.SomeSetting.ShouldBe(1));
        }


        // ENDSAMPLE


        // SAMPLE: can_customize_based_on_only_configuration
        [Fact]
        public void can_customize_based_on_only_configuration()
        {
            using (var runtime = JasperHost.For<UsingConfigApp>())
            {
                runtime.ServiceName.ShouldBe("WocketInMyPocket");
            }

        }

    }
}
