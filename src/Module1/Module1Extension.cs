using System;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Configuration;

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

            registry.Services.For<IBusLogger>().Add<ModuleBusLogger>();
        }

        public static JasperRegistry Registry { get; set; }
    }

    public interface IModuleService
    {

    }

    public class ModuleBusLogger : IBusLogger
    {
        public void Sent(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public void Received(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public void ExecutionStarted(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public void ExecutionFinished(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public void MessageSucceeded(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void LogException(Exception ex, string correlationId = null, string message = "Exception detected:")
        {
            throw new NotImplementedException();
        }

        public void NoHandlerFor(Envelope envelope)
        {
            throw new NotImplementedException();
        }
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
