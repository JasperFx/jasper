using Jasper.Testing.Settings;
using Microsoft.Extensions.Configuration;

namespace Jasper.Testing.Samples
{
    public class SampleApp : JasperRegistry
    {
        public bool MyBoolean { get; set; }

        public SampleApp()
        {
            // SAMPLE: alter-settings
            Settings.Alter<Jasper.Testing.Settings.MyFakeSettings>(_ =>
            {
                _.SomeSetting = 5;
            });

            // or additionally use IConfiguration
            Settings.Alter<MyFakeSettings>((config, settings) =>
            {
                settings.SomeSetting = int.Parse(config["SomeKey"]);
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
            Configuration.SetBasePath("path")
                .AddJsonFile("myconfig.json")
                .AddJsonFile("myotherconfig.json.config")
                .AddEnvironmentVariables();

            // ENDSAMPLE

            // SAMPLE: configure-settings
            Settings.Require<Colors>();
            // ENDSAMPLE

            // SAMPLE: configure-settings2
            Settings.Configure<MySettings>(_ => _.GetSection("subsection"));
            // ENDSAMPLE
        }

        // SAMPLE: inject-settings
        public class MyApp : JasperRegistry
        {
            public MyApp()
            {
                Configuration.AddJsonFile("mysettings.json");
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

        // SAMPLE: with-settings
        public class MyApplication : JasperRegistry
        {
            public bool MyBoolean { get; set; }

            public MyApplication()
            {
                Settings.With<MyFakeSettings>(_ =>
                {
                    if (_.SomeSetting == 1)
                    {
                        MyBoolean = true;
                    }
                });
            }
        }
        // ENDSAMPLE
    }




}
