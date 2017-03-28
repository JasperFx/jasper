using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Settings.Alter<MySettings>(_ =>
            {
                _.SomeSetting = 5;
            });
            // ENDSAMPLE

            // SAMPLE: replace-settings
            Settings.Replace(new MySettings
            {
                SomeSetting = 3,
                OtherSetting = "blue"
            });
            // ENDSAMPLE

            // SAMPLE: build-configuration
            Settings.Build(_ =>
            {
                _.AddJsonFile("myconfig.json");
                _.AddJsonFile("myotherconfig.json.config");
                _.AddEnvironmentVariables();
            });
            // ENDSAMPLE

            // SAMPLE: configure-settings
            Settings.Configure<MySettings>(_ => _.GetSection("subsection"));
            // ENDSAMPLE
        }

        // SAMPLE: inject-settings
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
                Settings.With<MySettings>(_ =>
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
