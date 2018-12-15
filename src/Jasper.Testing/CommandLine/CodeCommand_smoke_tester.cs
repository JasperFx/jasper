using Jasper.CommandLine;
using Jasper.Messaging.Configuration;
using Xunit;

namespace Jasper.Testing.CommandLine
{
    public class CodeCommand_smoke_tester
    {
        [Fact]
        public void can_generate_code()
        {
            var input = new CodeInput();
            var registry = new JasperRegistry();
            input.Source = new RegistryRuntimeSource(registry);


            registry.Handlers.DisableConventionalDiscovery();
            registry.Handlers.IncludeType<MessageConsumer>();

            var cmd = new CodeCommand();
            cmd.Execute(input);
        }
    }

    public class Message1
    {
    }

    public class MessageConsumer
    {
        public void Handle(Message1 message)
        {
        }
    }

    public static class HandlerConfigurationExtensions
    {
        public static IHandlerConfiguration DisableConventionalDiscovery(this IHandlerConfiguration handlers,
            bool disabled = true)
        {
            if (disabled) handlers.Discovery(x => x.DisableConventionalDiscovery());

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
    }
}
