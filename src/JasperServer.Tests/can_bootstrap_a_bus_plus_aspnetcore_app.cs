using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper;
using JasperBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Shouldly;
using StructureMap;
using Xunit;

namespace JasperServer.Tests
{
    public class can_bootstrap_a_bus_plus_aspnetcore_app : IDisposable
    {
        private readonly JasperRuntime theRuntime = JasperRuntime.For<JasperServerApp>();

        public void Dispose()
        {
            theRuntime.Dispose();
        }

        [Fact]
        public async Task can_handle_an_http_request_through_Kestrel()
        {
            using (var client = new HttpClient())
            {
                var text = await client.GetStringAsync("http://localhost:3002");
                text.ShouldContain("Hello from a hybrid Jasper application");

            }
        }

        [Fact]
        public void has_the_bus()
        {
            theRuntime.Container.GetInstance<IServiceBus>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void captures_registrations_from_configure_registry()
        {
            theRuntime.Container.GetInstance<IFoo>().ShouldBeOfType<Foo>();
        }
    }

    public class SomeHandler
    {
        public void Handle(SomeMessage message)
        {
            Console.WriteLine("Got a SomeMessage");
        }
    }

    public class SomeMessage
    {

    }

    public class JasperServerApp : JasperBusWithAspNetCoreRegistry
    {
        public JasperServerApp()
        {
            UseStartup<Startup>();
            Hosting.Port = 3002;
        }
    }

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(c => c.Response.WriteAsync("Hello from a hybrid Jasper application"));
        }

        public void ConfigureContainer(Registry registry)
        {
            registry.For<IFoo>().Use<Foo>();
        }
    }

    public interface IFoo{}
    public class Foo : IFoo{}
}
