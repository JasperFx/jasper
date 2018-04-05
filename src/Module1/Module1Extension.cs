using System;
using Jasper;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Subscriptions;

namespace Module1
{
    public class Module1Extension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            Registry = registry;

            registry.Settings.Alter<ModuleSettings>(_ =>
            {
                _.From = "Module1";
                _.Count = 100;
            });

            registry.Services.For<IModuleService>().Use<ServiceFromModule>();

        }

        public static JasperRegistry Registry { get; set; }
    }

    public interface IModuleService
    {

    }

    public class ModuleSettings
    {
        public string From { get; set; } = "Default";
        public int Count { get; set; }
    }

    public class ServiceFromModule : IModuleService
    {

    }
}
