using Jasper.Bus.Configuration;
using Jasper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Bus.Tracking
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
            registry.Logging.LogMessageEventsWith<MessageTrackingLogger>();
        }
    }
}
