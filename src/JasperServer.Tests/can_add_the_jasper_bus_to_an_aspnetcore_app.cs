using System;
using System.Net.Http;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Http;
using JasperBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace JasperServer.Tests
{
    public class can_add_the_jasper_bus_to_an_aspnetcore_app : IDisposable
    {
        private IWebHost theHost;

        public can_add_the_jasper_bus_to_an_aspnetcore_app()
        {
            var builder = new WebHostBuilder();
            builder
                .UseKestrel()
                .UseUrls("http://localhost:3003")
                .UseStartup<Startup>()
                .UseJasper<SimpleJasperBusApp>();

            theHost = builder.Build();
            theHost.Start();

        }

        public void Dispose()
        {
            theHost?.Dispose();
        }

        [Fact]
        public async Task can_handle_an_http_request_through_Kestrel()
        {
            using (var client = new HttpClient())
            {
                var text = await client.GetStringAsync("http://localhost:3003");
                text.ShouldContain("Hello from a hybrid Jasper application");

            }
        }

        [Fact]
        public void has_the_bus()
        {
            theHost.Services.GetService(typeof(IServiceBus))
                .ShouldNotBeNull();
        }

        [Fact]
        public void captures_registrations_from_configure_registry()
        {
            theHost.Services.GetService(typeof(IFoo)).ShouldBeOfType<Foo>();
        }

        [Fact]
        public void is_definitely_using_a_StructureMap_service_provider()
        {
            theHost.Services.ShouldBeOfType<StructureMapServiceProvider>();
        }
    }

    public class SimpleJasperBusApp : JasperBusRegistry
    {
        
    }


}

