using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Jasper.Testing.Bootstrapping
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

            Host.Get<IContainer>().Model.For<SomeHandler>().Default
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

    public class SomeMessage
    {
    }

    public class SomeHandler
    {
        public void Handle(SomeMessage message)
        {
        }
    }
}
