using Jasper.Testing.Settings;
using Microsoft.Extensions.Configuration;

namespace Jasper.Testing.Samples
{
    public class SampleApp : JasperRegistry
    {
        public SampleApp()
        {
            // SAMPLE: alter-settings
            Settings.Alter<MyFakeSettings>(_ => { _.SomeSetting = 5; });

            // or additionally use IConfiguration
            Settings.Alter<MyFakeSettings>((context, settings) =>
            {
                settings.SomeSetting = int.Parse(context.Configuration["SomeKey"]);
            });


            // ENDSAMPLE

            // SAMPLE: replace-settings
            Settings.Replace(new MyFakeSettings
            {
                SomeSetting = 3,
                OtherSetting = "blue"
            });
            // ENDSAMPLE

            // SAMPLE: build-configuration
            Hosting.ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(context.HostingEnvironment.WebRootPath)
                    .AddJsonFile("myconfig.json")
                    .AddJsonFile("myotherconfig.json.config")
                    .AddEnvironmentVariables();
            });



            // ENDSAMPLE

            // SAMPLE: configure-settings
            Settings.Require<Colors>();
            // ENDSAMPLE

            // SAMPLE: configure-settings2
            Settings.Configure<MySettings>(_ => _.GetSection("subsection"));
            // ENDSAMPLE
        }

        public bool MyBoolean { get; set; }

        // SAMPLE: inject-settings
        public class MyApp : JasperRegistry
        {
            public MyApp()
            {
                Hosting.ConfigureAppConfiguration((context, config) => config.AddJsonFile("mysettings.json"));
            }
        }


        public class SettingsTest
        {
            private readonly MySettings _settings;

            public SettingsTest(MySettings settings)
            {
                _settings = settings;
            }
        }
        // ENDSAMPLE

        public class MySettings{}
    }
}
