using System.Linq;
using Jasper.Configuration;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class import_handlers_with_extension : BootstrappingContext
    {
        [Fact]
        public void picks_up_on_handlers_from_extension()
        {
            theOptions.Extensions.Include<MyExtension>();

            var handlerChain = (theHandlers()).HandlerFor<ExtensionMessage>().Chain;
            handlerChain.Handlers.Single()
                .HandlerType.ShouldBe(typeof(ExtensionThing));
        }
    }

    public class MyExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Handlers.IncludeType<ExtensionThing>();
        }
    }

    public class ExtensionMessage
    {
    }

    public class ExtensionThing
    {
        public void Handle(ExtensionMessage message)
        {
        }
    }
}
