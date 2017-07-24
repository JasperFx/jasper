using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Testing.Bus.Compilation;
using Jasper.Testing.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.AspNetCoreIntegration
{
    public class can_bootstrap_a_bus_plus_aspnetcore_app_through_jasper_registry : IDisposable
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
                "Hello from a hybrid Jasper application".ShouldContain(text);

            }
        }

        [Fact]
        public void has_the_bus()
        {
            theRuntime.Container.GetInstance<IServiceBus>().ShouldNotBeNull();
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
            Services.ForSingletonOf<IFakeStore>().Use<FakeStore>();
            Services.AddService<IFakeService, FakeService>();
            Services.AddService<IWidget, Widget>(); 
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IFoo, Foo>();
        }
    }

    public interface IFoo{}
    public class Foo : IFoo{}
}
