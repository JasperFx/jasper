using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jasper.Testing.Settings
{
    public class MyApp : JasperRegistry
    {
        public bool MySetting { get; set; }

        public MyApp()
        {
            Settings.With<MySettings>(_ =>
            {
                if (_.SomeSetting != int.MaxValue)
                {
                    MySetting = true;
                }
            });
        }
    }
}
