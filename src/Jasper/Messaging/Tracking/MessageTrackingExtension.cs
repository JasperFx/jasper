using Jasper.Configuration;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Messaging.Tracking
{
    /// <summary>
    /// Add the message tracking support to your application
    /// for reliable automated testing
    /// </summary>
    public class MessageTrackingExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.AddSingleton<MessageHistory>();
            registry.Services.AddSingleton<IMessageLogger, MessageTrackingLogger>();
        }
    }
}
