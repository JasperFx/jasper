using System;
using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Configuration;
using Module1;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{

    public class discovering_and_using_extensions : IDisposable
    {
        private JasperRuntime theRuntime;

        public discovering_and_using_extensions()
        {
            theRuntime = JasperRuntime.For<AppWithOverrides>();
        }

        public void Dispose()
        {
            theRuntime.Dispose();
        }

        [Fact]
        public void can_discover_modules_from_assembly_scanning_and_apply_extensions()
        {
            Module1.Module1Extension.Registry = null;

            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.ShouldNotBeNull();

                Module1Extension.Registry.ShouldBe(registry);
            }
        }

        [Fact]
        public void can_inject_services_from_the_extension()
        {
            theRuntime.Container.Model.For<IBusLogger>()
                .Instances.Any(x => x.ReturnedType == typeof(ModuleBusLogger))
                .ShouldBeTrue();
        }

        [Fact]
        public void application_service_registrations_win()
        {
            theRuntime.Container.Model.DefaultTypeFor<IModuleService>()
                .ShouldBe(typeof(AppsModuleService));
        }

        [Fact]
        public void extension_can_alter_settings()
        {
            var instance = theRuntime.Container.Model.For<ModuleSettings>()
                .Default;

            Console.WriteLine(theRuntime.Container.WhatDoIHave());

            // This value comes from Module1Extension
            var moduleSettings = theRuntime.Container.GetInstance<ModuleSettings>();
            moduleSettings
                .Count.ShouldBe(100);
        }

        [Fact]
        public void application_settings_alterations_win()
        {
            theRuntime.Container.GetInstance<ModuleSettings>()
                .From.ShouldBe("Application");
        }
    }



    public class AppWithOverrides : JasperRegistry
    {
        public AppWithOverrides()
        {
            Handlers.DisableConventionalDiscovery(true);

            Settings.Alter<ModuleSettings>(_ => _.From = "Application");

            Services.For<IModuleService>().Use<AppsModuleService>();


        }
    }

    public class AppsModuleService : IModuleService{}



}
