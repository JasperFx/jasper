using System;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
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

            registry.Services.For<IMessageLogger>().Use<ModuleMessageLogger>();
        }

        public static JasperRegistry Registry { get; set; }
    }

    public interface IModuleService
    {

    }

    public class ModuleMessageLogger : IMessageLogger
    {
        public void MovedToErrorQueue(Envelope envelope, Exception ex)
        {

        }

        public void Sent(Envelope envelope)
        {
        }

        public void Received(Envelope envelope)
        {
        }

        public void ExecutionStarted(Envelope envelope)
        {
        }

        public void ExecutionFinished(Envelope envelope)
        {
        }

        public void MessageSucceeded(Envelope envelope)
        {
        }

        public void MessageFailed(Envelope envelope, Exception ex)
        {
        }

        public void LogException(Exception ex, Guid correlationId = default(Guid), string message = "Exception detected:")
        {
        }

        public void NoHandlerFor(Envelope envelope)
        {
        }

        public void NoRoutesFor(Envelope envelope)
        {

        }

        public void SubscriptionMismatch(PublisherSubscriberMismatch mismatch)
        {

        }

        public void Undeliverable(Envelope envelope)
        {

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
