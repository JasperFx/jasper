using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.Http;

namespace Jasper.Testing.Settings
{
    public class MyApp : JasperRegistry
    {
        public bool MySetting { get; set; }

        public MyApp()
        {
            Services.AddService<IFakeStore, FakeStore>();
            Services.For<IWidget>().Use<Widget>();
            Services.For<IFakeService>().Use<FakeService>();

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
