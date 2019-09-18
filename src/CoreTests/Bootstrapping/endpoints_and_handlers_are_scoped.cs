using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CoreTests.Bootstrapping
{
    public class endpoints_and_handlers_are_scoped : IntegrationContext
    {
        public endpoints_and_handlers_are_scoped(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void handler_classes_are_scoped()
        {
            // forcing the container to resolve the family
            var endpoint = Host.Get<SomeHandler>();

            Host.Container.Model.For<SomeHandler>().Default
                .Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }
    }

    public class SomeEndpoint
    {
        public string get_something()
        {
            return "something";
        }
    }

    public class SomeMessage{}

    public class SomeHandler
    {
        public void Handle(SomeMessage message){}
    }
}
