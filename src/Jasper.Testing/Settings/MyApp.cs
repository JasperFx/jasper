using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.FakeStoreTypes;
using Jasper.Testing.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Settings
{
    public class MyApp : JasperRegistry
    {
        public bool MySetting { get; set; }

        public MyApp()
        {
            Services.AddTransient<IFakeStore, FakeStore>();
            Services.For<IWidget>().Use<Widget>();
            Services.For<IFakeService>().Use<FakeService>();

            Settings.With<MyFakeSettings>(_ =>
            {
                if (_.SomeSetting != int.MaxValue)
                {
                    MySetting = true;
                }
            });
        }
    }
}
