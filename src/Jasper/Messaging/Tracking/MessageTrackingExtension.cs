using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Lamar;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Messaging.Tracking
{
    /// <summary>
    ///     Add the message tracking support to your application
    ///     for reliable automated testing
    /// </summary>
    public class MessageTrackingExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Services.For<IMessageLogger>().Use<MessageTrackingLogger>().Singleton();
        }
    }

    public static class MessageTrackingExtensions
    {
        /// <summary>
        /// Add the message tracking support to your application
        /// for reliable automated testing
        /// </summary>
        /// <param name="extensions"></param>
        public static void UseMessageTrackingTestingSupport(this IExtensions extensions)
        {
            extensions.Include<MessageTrackingExtension>();
        }
    }
}
