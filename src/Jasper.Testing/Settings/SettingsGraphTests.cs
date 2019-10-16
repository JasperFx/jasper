using System;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Settings
{
    public class SettingsGraphTests
    {
        public SettingsGraphTests()
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





        [Fact]
        public void can_alter_settings()
        {
            theRegistry.Settings.Alter<MyFakeSettings>(s => { s.SomeSetting = 5; });


            with<MyFakeSettings>(x => x.SomeSetting.ShouldBe(5));
        }

    }
}
