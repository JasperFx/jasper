using System;
using Jasper.Messaging.Configuration;

namespace IntegrationTests.Persistence.Marten
{
    public static class HandlerConfigurationExtensions
    {
        public static IHandlerConfiguration DisableConventionalDiscovery(this IHandlerConfiguration handlers)
        {
            handlers.Discovery(x => x.DisableConventionalDiscovery());

            return handlers;
        }

        public static IHandlerConfiguration OnlyType<T>(this IHandlerConfiguration handlers)
        {
            handlers.Discovery(x =>
            {
                x.DisableConventionalDiscovery();
                x.IncludeType<T>();
            });

            return handlers;
        }

        public static IHandlerConfiguration IncludeType<T>(this IHandlerConfiguration handlers)
        {
            handlers.Discovery(x => x.IncludeType<T>());

            return handlers;
        }

        public static IHandlerConfiguration IncludeType(this IHandlerConfiguration handlers, Type handlerType)
        {
            handlers.Discovery(x => x.IncludeType(handlerType));

            return handlers;
        }
    }
}
